using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Identity;

namespace SampleFlow.Infrastructure.Data.Configurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");

        // مفتاح مركّب: منح/سحب واحد لكل (مستخدم، صلاحية).
        builder.HasKey(up => new { up.UserId, up.PermissionId });

        // الصلاحية → استثناءات المستخدمين.
        builder.HasOne(up => up.Permission)
            .WithMany(p => p.UserPermissions)
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // المستخدم (FK → AspNetUsers). حذف المستخدم يزيل استثناءاته.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // لتسريع البحث عن كل من له استثناء على صلاحية معيّنة.
        builder.HasIndex(up => up.PermissionId);
    }
}
