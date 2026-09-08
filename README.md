<div dir="rtl">

# SampleFlow — نظام متابعة العينات اليومية

نظام ويب لتجميع بيانات العينات اليومية من عدة مراكز صحية في مكان واحد. كل مركز
يُدخل بياناته مباشرة، والتجميع يصير **تلقائياً** في لوحة المشرف — بدل تجميع ملفات
Excel يدوياً.

## المزايا
- إدخال يومي لكل مركز مع **منع التكرار** (إدخال واحد لكل مركز/يوم).
- **حقول تحاليل ديناميكية** تُدار من لوحة الإدارة دون تعديل الكود.
- لوحة مشرف: مؤشّرات، فلترة، جدول تجميعي، ورسوم.
- **تصدير Excel و PDF** (عربي RTL).
- **صلاحيات دقيقة** مخزّنة في قاعدة البيانات وتُطبّق فوراً عبر سياسات ديناميكية.
- **سجل تتبّع** تلقائي لكل إضافة/تعديل/حذف/دخول.

## التقنيات
ASP.NET Core 8 (MVC) · PostgreSQL · Entity Framework Core · ASP.NET Core Identity
· ClosedXML (Excel) · QuestPDF (PDF) · واجهة عربية RTL.

## بنية المشروع
```
SampleFlow.sln
src/
├── SampleFlow.Domain/          الكيانات وثوابت الصلاحيات (بلا تبعيات)
├── SampleFlow.Infrastructure/  DbContext، الهجرات، البذر، اعتراض التتبّع
└── SampleFlow.Web/             MVC: المصادقة، التفويض، الشاشات، التصدير
docs/                           الوثائق (انظر أدناه)
```

## التشغيل محلياً

**المتطلبات:** .NET 8 SDK · PostgreSQL · أداة `dotnet-ef`
(`dotnet tool install --global dotnet-ef`).

```bash
# 1) اضبط سلسلة الاتصال كسرّ (لا تضعها في appsettings.json)
cd src/SampleFlow.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=sampleflow;Username=postgres;Password=<كلمة-المرور>"

# 2) بيانات مستخدم الأدمن الأولي (اختياري لكن مُوصى)
dotnet user-secrets set "Seed:AdminEmail" "admin@yourdomain.com"
dotnet user-secrets set "Seed:AdminPassword" "<كلمة-مرور-قوية>"

# 3) طبّق الهجرات (تنشئ الجداول وتبذر البيانات المرجعية)
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=sampleflow;Username=postgres;Password=<كلمة-المرور>"
dotnet ef database update \
  --project src/SampleFlow.Infrastructure --startup-project src/SampleFlow.Infrastructure

# 4) شغّل
dotnet run --project src/SampleFlow.Web
```

يُبذَر عند الإقلاع: 16 مركزاً، 9 أنواع تحاليل، 11 صلاحية، 3 أدوار، ومستخدم أدمن.
سجّل الدخول بحساب الأدمن الذي ضبطته (الافتراضي `admin@sampleflow.local` /
`ChangeMe!123` — **غيّره فوراً**).

## الأدوار
- **Center** — يُدخل بيانات مركزه ويشاهد إدخالاته.
- **Supervisor** — يشاهد كل المراكز والتقارير والتصدير.
- **Admin** — كل ما سبق + إدارة المراكز والتحاليل والمستخدمين والصلاحيات وسجل التتبّع.

## الوثائق
- [`docs/USER_GUIDE.md`](docs/USER_GUIDE.md) — دليل المستخدم النهائي.
- [`docs/DATABASE.md`](docs/DATABASE.md) — الهجرات والبذر.
- [`docs/AUTH.md`](docs/AUTH.md) — المصادقة والتفويض بالصلاحيات.
- [`docs/AUDITING.md`](docs/AUDITING.md) — سجل التتبّع التلقائي.
- [`PROJECT_SPEC.md`](PROJECT_SPEC.md) — المواصفات الكاملة.

## ملاحظات أمنية
- لا تضع كلمات المرور الحقيقية في `appsettings.json` — استخدم User Secrets
  (تطوير) أو متغيّرات البيئة (إنتاج).
- فعّل HTTPS في الإنتاج، وخذ نسخاً احتياطية دورية لقاعدة البيانات.

## الترخيص والأصول
خط **Amiri** (لتصدير PDF) مرخّص بـ SIL Open Font License — انظر
`src/SampleFlow.Web/Fonts/OFL.txt`.

</div>
