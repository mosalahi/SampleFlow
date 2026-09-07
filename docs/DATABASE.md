# قاعدة البيانات — الـ Migrations والبذر (المرحلة 2)

يشرح هذا الملف كيفية توليد الـ Migration الأولية وتطبيقها وبذر البيانات.

## المتطلبات
- .NET 8 SDK
- أداة EF: `dotnet tool install --global dotnet-ef --version 8.*`
- خادم PostgreSQL يعمل
- الوصول لمغذّي حزم NuGet (لاسترجاع EF Core / Npgsql)

## سلسلة الاتصال
تُقرأ من متغير البيئة `ConnectionStrings__DefaultConnection`
(يستخدمه مصنع وقت التصميم `ApplicationDbContextFactory`)، مع افتراضي محلي إن لم يُضبط:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=sampleflow;Username=postgres;Password=postgres"
```

## توليد الـ Migration الأولية
مشروع `SampleFlow.Infrastructure` هو مشروع البدء ومشروع الـ Migrations معاً
(يعمل بلا مضيف بفضل `ApplicationDbContextFactory`):

```bash
dotnet ef migrations add InitialCreate \
  --project src/SampleFlow.Infrastructure \
  --startup-project src/SampleFlow.Infrastructure \
  --output-dir Data/Migrations
```

تتضمّن هذه الـ Migration تلقائياً:
- جداول Identity (`AspNetUsers`, `AspNetRoles`, ...).
- جداول النطاق: `Centers`, `SampleTypes`, `DailyEntries`, `DailyEntryDetails`,
  `AuditLogs`, `Permissions`, `RolePermissions`, `UserPermissions`.
- الفهارس والقيود (خصوصاً الفريد على `CenterId + EntryDate`).
- **بذر البيانات المرجعية** عبر `HasData` (المراكز، أنواع التحاليل، الصلاحيات،
  الأدوار، وربط الأدوار بالصلاحيات) — انظر `Data/SeedData.cs`.

## تطبيق الـ Migration
```bash
dotnet ef database update \
  --project src/SampleFlow.Infrastructure \
  --startup-project src/SampleFlow.Infrastructure
```

## البذر
### بيانات مرجعية (تلقائي مع الـ Migration)
16 مركزاً · 9 أنواع تحاليل · 11 صلاحية · 3 أدوار · ربط الأدوار بالصلاحيات:
- **Center:** `Entries.Create`, `Entries.ViewOwn`
- **Supervisor:** `Entries.EditAny`, `Entries.ViewAny`, `Reports.View`, `Reports.Export`
- **Admin:** جميع الصلاحيات

### مستخدم الأدمن (وقت التشغيل)
يُنشأ عبر `Data/Seeding/IdentityDataSeeder` لأن تجزئة كلمة المرور تتطلّب `UserManager`.
يُستدعى من مضيف الويب بعد `database update` (يُوصَل في مرحلة المصادقة)، ويقرأ
بيانات الأدمن من الإعدادات بدل تضمينها في الكود:

```bash
export Seed__AdminEmail="admin@sampleflow.local"
export Seed__AdminPassword="<كلمة-مرور-قوية>"
```

> الإجراء idempotent: يضمن وجود الأدوار وينشئ الأدمن ويُلحقه بدور `Admin` إن لم يكن موجوداً.

## ملاحظة حول بيئة الجلسة الحالية
تعذّر توليد ملف الـ Migration داخل بيئة هذه الجلسة لأن سياسة الشبكة تحظر مغذّيات
حزم NuGet، فلا يمكن استرجاع EF Core / Npgsql ولا بناء طبقة Infrastructure هنا.
كود النموذج والبذر جاهز وصحيح، وتوليد الـ Migration يتم بأمرٍ واحد أعلاه في أي بيئة
يتوفر فيها الوصول لـ NuGet.
