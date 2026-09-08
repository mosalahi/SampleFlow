using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using SampleFlow.Infrastructure.Auditing;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Infrastructure.Data.Seeding;
using SampleFlow.Infrastructure.DependencyInjection;
using SampleFlow.Infrastructure.Identity;
using SampleFlow.Web.Authorization;
using SampleFlow.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF: ترخيص Community + تسجيل خطوط Amiri العربية (RTL) للتصدير.
QuestPDF.Settings.License = LicenseType.Community;
var fontsDir = Path.Combine(builder.Environment.ContentRootPath, "Fonts");
if (Directory.Exists(fontsDir))
{
    foreach (var ttf in Directory.GetFiles(fontsDir, "*.ttf"))
    {
        using var fontStream = File.OpenRead(ttf);
        FontManager.RegisterFont(fontStream);
    }
}

builder.Services.AddControllersWithViews();

// مصدر معلومات التتبّع من الطلب الحالي — يُسجَّل قبل الاستمرارية ليأخذ الأولوية.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditInfoProvider, HttpAuditInfoProvider>();

// السياق + PostgreSQL + اعتراض التتبّع + بذر Identity.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("سلسلة الاتصال 'DefaultConnection' غير مضبوطة.");
builder.Services.AddSampleFlowPersistence(connectionString);

// ASP.NET Core Identity.
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// خدمة تجميع لوحة المشرف (تُستخدم في اللوحة والتصدير).
builder.Services.AddScoped<IDashboardService, DashboardService>();

// تفويض مبني على الصلاحيات عبر سياسات ديناميكية.
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// بذر الأدوار ومستخدم الأدمن (idempotent). يقرأ البيانات من الإعدادات.
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@sampleflow.local";
    var adminPassword = configuration["Seed:AdminPassword"] ?? "ChangeMe!123";

    await seeder.SeedAsync(adminEmail, adminPassword);
}

app.Run();
