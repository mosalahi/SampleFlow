using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SampleFlow.Domain.Entities;

namespace SampleFlow.Infrastructure.Auditing;

/// <summary>
/// يعترض <c>SaveChanges</c> ويكتب صف <see cref="AuditLog"/> لكل عملية
/// Create/Update/Delete تلقائياً (تتبّع مركزي).
///
/// آلية العمل:
/// 1) قبل الحفظ (<c>SavingChanges</c>): يلتقط الكيانات المتغيّرة والقيم القديمة/الجديدة.
///    المفاتيح المولّدة تلقائياً تكون "مؤقتة" فتُؤجَّل.
/// 2) بعد الحفظ (<c>SavedChanges</c>): يملأ القيم المؤجّلة (صارت متاحة)، ثم يضيف
///    صفوف AuditLog ويحفظها. الالتقاط يتجاهل AuditLog نفسه، فلا يحدث استدعاء متكرّر.
///
/// يُسجَّل بعمر Scoped (نسخة لكل سياق/طلب). لا يُسجّل الدخول (Login) — يُسجَّل يدوياً
/// من مسار المصادقة لأنه ليس تغييراً على القاعدة.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IAuditInfoProvider _auditInfo;
    private List<AuditEntry> _pending = new();

    public AuditSaveChangesInterceptor(IAuditInfoProvider auditInfo) => _auditInfo = auditInfo;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            _pending = Capture(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            _pending = Capture(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is not null && _pending.Count > 0)
        {
            var context = eventData.Context;
            context.Set<AuditLog>().AddRange(BuildLogs());
            context.SaveChanges(); // آمن: الالتقاط يتجاهل AuditLog فلا تكرار.
        }

        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null && _pending.Count > 0)
        {
            var context = eventData.Context;
            await context.Set<AuditLog>().AddRangeAsync(BuildLogs(), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditEntry> Capture(DbContext context)
    {
        context.ChangeTracker.DetectChanges();

        var entries = new List<AuditEntry>();
        foreach (var entry in context.ChangeTracker.Entries())
        {
            // تجاهل AuditLog نفسه، والحالات التي لا تمثّل تغييراً.
            if (entry.Entity is AuditLog)
                continue;
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var audit = new AuditEntry(entry)
            {
                EntityName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name,
                Action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Deleted => "Delete",
                    _ => "Update",
                },
            };

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    // القيمة تُحسم بعد الحفظ (مفتاح مولّد) — تُؤجَّل.
                    audit.TemporaryProperties.Add(property);
                    continue;
                }

                var name = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    audit.KeyValues[name] = property.CurrentValue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        audit.NewValues[name] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        audit.OldValues[name] = property.OriginalValue;
                        break;

                    case EntityState.Modified when property.IsModified:
                        audit.OldValues[name] = property.OriginalValue;
                        audit.NewValues[name] = property.CurrentValue;
                        break;
                }
            }

            entries.Add(audit);
        }

        return entries;
    }

    private List<AuditLog> BuildLogs()
    {
        var logs = new List<AuditLog>(_pending.Count);
        foreach (var audit in _pending)
        {
            // ملء القيم التي كانت مؤقتة (صارت متاحة بعد الحفظ).
            foreach (var property in audit.TemporaryProperties)
            {
                var name = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    audit.KeyValues[name] = property.CurrentValue;
                }

                if (audit.Action != "Delete")
                {
                    audit.NewValues[name] = property.CurrentValue;
                }
            }

            logs.Add(audit.ToAuditLog(_auditInfo));
        }

        _pending = new List<AuditEntry>();
        return logs;
    }
}
