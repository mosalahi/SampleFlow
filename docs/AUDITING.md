# سجل التتبّع (AuditLog) — المرحلة 3

تسجيل تلقائي لكل عمليات Create / Update / Delete عبر اعتراض `SaveChanges`.

## المكوّنات
| الملف | الدور |
|---|---|
| `Auditing/AuditSaveChangesInterceptor.cs` | يعترض الحفظ ويكتب صفوف `AuditLogs` |
| `Auditing/IAuditInfoProvider.cs` | مصدر المستخدم الحالي + IP (تنفّذه طبقة الويب) |
| `Auditing/NullAuditInfoProvider.cs` | افتراضي فارغ (بلا سياق طلب) |
| `Auditing/AuditEntry.cs` | حاوية التقاط مؤقتة |
| `DependencyInjection/InfrastructureServiceCollectionExtensions.cs` | تسجيل السياق + الاعتراض |

## ما يُلتقط
- **Action:** Create / Update / Delete (الدخول `Login` يُسجَّل يدوياً من مسار المصادقة).
- **EntityName / EntityId:** اسم الجدول ومفتاح السجل (بما فيه المفاتيح المولّدة بعد الحفظ).
- **OldValues / NewValues:** JSON بالقيم قبل/بعد (للتعديل تُسجَّل الخصائص المتغيّرة فقط).
- **UserId / UserName / IpAddress:** من `IAuditInfoProvider`.
- **Timestamp:** UTC.

> الاعتراض يتجاهل جدول `AuditLogs` نفسه، فلا يحدث استدعاء متكرّر.

## التفعيل (في طبقة الويب — المرحلة 4)
```csharp
// 1) تنفيذ المزوّد من HttpContext (يُسجَّل قبل الاستمرارية ليأخذ الأولوية):
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditInfoProvider, HttpAuditInfoProvider>();

// 2) تسجيل السياق + الاعتراض:
builder.Services.AddSampleFlowPersistence(
    builder.Configuration.GetConnectionString("DefaultConnection")!);
```
بعدها كل `context.SaveChanges()` يكتب سجلات التتبّع تلقائياً دون أي كود إضافي في المتحكّمات.
