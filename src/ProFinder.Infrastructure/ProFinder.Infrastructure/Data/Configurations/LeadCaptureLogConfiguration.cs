using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class LeadCaptureLogConfiguration : IEntityTypeConfiguration<LeadCaptureLog>
{
    public void Configure(EntityTypeBuilder<LeadCaptureLog> builder)
    {
        builder.ToTable(TableNames.LeadCaptureLogs);

        builder.Property(x => x.LogLevel)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(x => x.LeadCaptureRunId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.LeadCaptureRun)
            .WithMany(x => x.Logs)
            .HasForeignKey(x => x.LeadCaptureRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
