using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleFlow.Domain.Entities;

namespace SampleFlow.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);

        // المفتاح فريد — يُستخدم كاسم السياسة في التفويض الديناميكي.
        builder.HasIndex(p => p.Key).IsUnique();

        // لتجميع الصلاحيات حسب التصنيف في الواجهة.
        builder.HasIndex(p => p.Category);
    }
}
