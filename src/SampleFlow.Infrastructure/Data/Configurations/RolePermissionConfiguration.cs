using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Identity;

namespace SampleFlow.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        // مفتاح مركّب: لا يتكرر ربط الدور بنفس الصلاحية.
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // الصلاحية → روابط الأدوار.
        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // الدور (FK → AspNetRoles). حذف الدور يزيل روابطه.
        builder.HasOne<ApplicationRole>()
            .WithMany()
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // لتسريع البحث بصلاحية معيّنة عبر كل الأدوار.
        builder.HasIndex(rp => rp.PermissionId);
    }
}
