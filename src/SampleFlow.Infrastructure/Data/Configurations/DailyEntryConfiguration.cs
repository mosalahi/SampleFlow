using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Identity;

namespace SampleFlow.Infrastructure.Data.Configurations;

public class DailyEntryConfiguration : IEntityTypeConfiguration<DailyEntry>
{
    public void Configure(EntityTypeBuilder<DailyEntry> builder)
    {
        builder.ToTable("DailyEntries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedByUserId)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        // القيد الجوهري: إدخال واحد لكل مركز/يوم.
        builder.HasIndex(e => new { e.CenterId, e.EntryDate })
            .IsUnique();

        // المركز → إدخالاته. Restrict: لا نحذف مركزاً له إدخالات (نعطّله بدلاً من ذلك).
        builder.HasOne(e => e.Center)
            .WithMany(c => c.DailyEntries)
            .HasForeignKey(e => e.CenterId)
            .OnDelete(DeleteBehavior.Restrict);

        // المنشئ (FK → AspNetUsers) بلا خاصية تنقّل عكسية، ومنع الحذف للحفاظ على الأثر.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // آخر معدّل (FK اختياري → AspNetUsers).
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
