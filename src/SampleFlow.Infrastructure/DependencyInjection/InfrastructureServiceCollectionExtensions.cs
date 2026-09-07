using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SampleFlow.Infrastructure.Auditing;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Infrastructure.Data.Seeding;

namespace SampleFlow.Infrastructure.DependencyInjection;

/// <summary>
/// تسجيل طبقة الوصول للبيانات: السياق مع PostgreSQL، اعتراض التتبّع، وبذر Identity.
/// تستدعيها طبقة الويب. تنفيذ <see cref="IAuditInfoProvider"/> الحقيقي (من HttpContext)
/// يُسجَّل في طبقة الويب؛ هنا نضع افتراضاً فارغاً فقط إن لم يُسجَّل غيره.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSampleFlowPersistence(
        this IServiceCollection services, string connectionString)
    {
        // افتراضي فارغ — تستبدله طبقة الويب بتسجيل أسبق يعتمد على HttpContext.
        services.TryAddScoped<IAuditInfoProvider, NullAuditInfoProvider>();

        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsAssembly(
                        typeof(ApplicationDbContext).Assembly.FullName))
                .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<IdentityDataSeeder>();

        return services;
    }
}
