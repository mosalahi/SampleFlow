using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleFlow.Domain.Entities;

namespace SampleFlow.Infrastructure.Data.Configurations;

public class DailyEntryDetailConfiguration : IEntityTypeConfiguration<DailyEntryDetail>
{
    public void Configure(EntityTypeBuilder<DailyEntryDetail> builder)
    {
        builder.ToTable("DailyEntryDetails");
        builder.HasKey(d => d.Id);

        // صف واحد لكل نوع تحليل داخل الإدخال.
        builder.HasIndex(d => new { d.DailyEntryId, d.SampleTypeId })
            .IsUnique();

        // حذف الإدخال يحذف تفاصيله (Cascade).
        builder.HasOne(d => d.DailyEntry)
            .WithMany(e => e.Details)
            .HasForeignKey(d => d.DailyEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: لا نحذف نوع تحليل مستخدَماً في تفاصيل (نعطّله بدلاً من ذلك).
        builder.HasOne(d => d.SampleType)
            .WithMany(s => s.Details)
            .HasForeignKey(d => d.SampleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
