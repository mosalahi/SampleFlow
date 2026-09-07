using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Identity;

namespace SampleFlow.Infrastructure.Data;

/// <summary>
/// سياق قاعدة البيانات. يرث IdentityDbContext ليولّد جداول Identity
/// (AspNetUsers/Roles ...) مع مستخدم/دور مخصّصين، ويضيف جداول النطاق.
/// </summary>
public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Center> Centers => Set<Center>();
    public DbSet<SampleType> SampleTypes => Set<SampleType>();
    public DbSet<DailyEntry> DailyEntries => Set<DailyEntry>();
    public DbSet<DailyEntryDetail> DailyEntryDetails => Set<DailyEntryDetail>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // يهيّئ جداول Identity أولاً.
        base.OnModelCreating(builder);

        // ثم يطبّق كل IEntityTypeConfiguration في هذا التجميع
        // (يشمل تهيئة ApplicationUser، فتُطبَّق بعد إعدادات Identity الأساسية).
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
