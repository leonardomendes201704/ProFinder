using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class GoogleMapsLeadConfiguration : IEntityTypeConfiguration<GoogleMapsLead>
{
    public void Configure(EntityTypeBuilder<GoogleMapsLead> builder)
    {
        builder.ToTable(TableNames.GoogleMapsLeads);

        builder.Property(x => x.SearchQuery)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PlaceUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        builder.Property(x => x.NormalizedPhone)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(300);

        builder.Property(x => x.Website)
            .HasMaxLength(300);

        builder.Property(x => x.Rating)
            .HasPrecision(3, 2);

        builder.Property(x => x.ImportStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RawPayloadJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.PlaceUrl)
            .IsUnique();

        builder.HasIndex(x => new { x.NormalizedPhone, x.Name });

        builder.HasOne(x => x.LeadCaptureRun)
            .WithMany(x => x.GoogleMapsLeads)
            .HasForeignKey(x => x.LeadCaptureRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LeadSource)
            .WithMany(x => x.GoogleMapsLeads)
            .HasForeignKey(x => x.LeadSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Profession)
            .WithMany()
            .HasForeignKey(x => x.ProfessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ImportedProfessional)
            .WithMany()
            .HasForeignKey(x => x.ImportedProfessionalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
