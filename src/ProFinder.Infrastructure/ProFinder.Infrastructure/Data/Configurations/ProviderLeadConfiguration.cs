using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class ProviderLeadConfiguration : IEntityTypeConfiguration<ProviderLead>
{
    public void Configure(EntityTypeBuilder<ProviderLead> builder)
    {
        builder.ToTable(TableNames.ProviderLeads);

        builder.Property(x => x.SiteKey)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.SearchQuery)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.DeduplicationKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        builder.Property(x => x.WhatsApp)
            .HasMaxLength(30);

        builder.Property(x => x.NormalizedPhone)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(300);

        builder.Property(x => x.Neighborhood)
            .HasMaxLength(120);

        builder.Property(x => x.City)
            .HasMaxLength(120);

        builder.Property(x => x.State)
            .HasMaxLength(10);

        builder.Property(x => x.Website)
            .HasMaxLength(250);

        builder.Property(x => x.SourceListingUrl)
            .HasMaxLength(500);

        builder.Property(x => x.SourceDetailsUrl)
            .HasMaxLength(500);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(120);

        builder.Property(x => x.ImportStatus)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.SourceSitesJson)
            .HasMaxLength(2000);

        builder.Property(x => x.SourceUrlsJson)
            .HasMaxLength(4000);

        builder.Property(x => x.RawPayloadJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Rating)
            .HasPrecision(5, 2);

        builder.HasIndex(x => x.DeduplicationKey)
            .IsUnique();

        builder.HasIndex(x => x.NormalizedPhone);
        builder.HasIndex(x => x.SiteKey);
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.ImportStatus);
        builder.HasIndex(x => x.LeadCaptureRunId);
        builder.HasIndex(x => x.LeadSourceId);

        builder.HasOne(x => x.LeadCaptureRun)
            .WithMany(x => x.ProviderLeads)
            .HasForeignKey(x => x.LeadCaptureRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeadSource)
            .WithMany(x => x.ProviderLeads)
            .HasForeignKey(x => x.LeadSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Profession)
            .WithMany(x => x.ProviderLeads)
            .HasForeignKey(x => x.ProfessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Region)
            .WithMany(x => x.ProviderLeads)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ImportedProfessional)
            .WithMany(x => x.ImportedProviderLeads)
            .HasForeignKey(x => x.ImportedProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
