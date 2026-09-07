using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleFlow.Infrastructure.Identity;

namespace SampleFlow.Infrastructure.Data.Configurations;

/// <summary>
/// تهيئة إضافية لـ ApplicationUser فوق ما يولّده Identity:
/// ربط المستخدم بمركزه. Restrict: لا نحذف مركزاً مرتبطاً بمستخدمين.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(u => u.Center)
            .WithMany()
            .HasForeignKey(u => u.CenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
