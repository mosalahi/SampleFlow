# المصادقة والتفويض (المرحلة 4)

## مشروع الويب
`src/SampleFlow.Web` — ASP.NET Core MVC (RTL، بهوية المرجع البصري).

## تسجيل الدخول
- الدخول بـ **اسم المستخدم أو البريد + كلمة المرور** عبر ASP.NET Core Identity.
  (قائمة المركز في المرجع البصري تصوّر فقط؛ عملياً حساب لكل مركز.)
- **توجيه حسب الدور** بعد الدخول: Supervisor/Admin ← لوحة المشرف، غيرهم ← شاشة الإدخال.
- كل دخول ناجح يُسجَّل في `AuditLogs` بإجراء `Login`.
- Lockout: 5 محاولات فاشلة → قفل 15 دقيقة.

## التفويض المبني على الصلاحيات (سياسات ديناميكية)
تُحمى الشاشات بالصلاحية لا بالدور:
```csharp
[Authorize(Policy = PermissionKeys.EntriesEditAny)]   // بدل [Authorize(Roles = "Admin")]
```
- `PermissionPolicyProvider` يبني سياسة لأي مفتاح صلاحية تلقائياً.
- `PermissionAuthorizationHandler` + `PermissionService` يحسبان الصلاحيات الفعّالة
  **من قاعدة البيانات في كل طلب**: صلاحيات أدوار المستخدم ± استثناءاته
  (`UserPermissions`: منح/سحب). أي تعديل صلاحية **يسري فوراً** دون إعادة تسجيل دخول.

## مستخدم الأدمن الأولي
يُبذَر عند الإقلاع (idempotent). اضبط بياناته قبل التشغيل — لا تتركه على الافتراضي في الإنتاج:
```bash
# Development: user-secrets أو متغيرات بيئة
setx Seed__AdminEmail "admin@yourdomain.com"
setx Seed__AdminPassword "كلمة-مرور-قوية"
```
> الافتراضي عند عدم الضبط: `admin@sampleflow.local` / `ChangeMe!123` — **غيّرها فوراً**.

## التشغيل
1. تأكد أن `ConnectionStrings:DefaultConnection` في `appsettings.json` يشير لقاعدتك.
2. طبّق الـ Migrations (المرحلة 2) إن لم تكن مطبّقة.
3. شغّل مشروع `SampleFlow.Web` (F5 أو `dotnet run`).
