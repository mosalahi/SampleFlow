using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SampleFlow.Infrastructure.Data;

/// <summary>
/// مصنع وقت التصميم — يُمكّن أدوات EF (dotnet ef migrations/update) من إنشاء
/// السياق دون مشروع مضيف. سلسلة الاتصال تُقرأ من متغير البيئة
/// <c>ConnectionStrings__DefaultConnection</c> مع افتراضي محلي.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=sampleflow;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(
                    typeof(ApplicationDbContextFactory).Assembly.FullName))
            .Options;

        return new ApplicationDbContext(options);
    }
}
