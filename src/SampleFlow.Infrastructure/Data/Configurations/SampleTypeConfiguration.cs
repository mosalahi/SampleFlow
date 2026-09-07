using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleFlow.Domain.Entities;

namespace SampleFlow.Infrastructure.Data.Configurations;

public class SampleTypeConfiguration : IEntityTypeConfiguration<SampleType>
{
    public void Configure(EntityTypeBuilder<SampleType> builder)
    {
        builder.ToTable("SampleTypes");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        // الرمز فريد.
        builder.HasIndex(s => s.Code).IsUnique();

        // لتسريع الترتيب في شاشة الإدخال ولوحة المشرف.
        builder.HasIndex(s => s.DisplayOrder);
    }
}
