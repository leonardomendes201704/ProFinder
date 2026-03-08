using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class LeadCaptureRunConfiguration : IEntityTypeConfiguration<LeadCaptureRun>
{
    public void Configure(EntityTypeBuilder<LeadCaptureRun> builder)
    {
        builder.ToTable(TableNames.LeadCaptureRuns);

        builder.Property(x => x.CaptureType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SourceUrl)
            .HasMaxLength(200);

        builder.Property(x => x.FileName)
            .HasMaxLength(200);

        builder.Property(x => x.SearchQuery)
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.HasOne(x => x.LeadSource)
            .WithMany(x => x.LeadCaptureRuns)
            .HasForeignKey(x => x.LeadSourceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
