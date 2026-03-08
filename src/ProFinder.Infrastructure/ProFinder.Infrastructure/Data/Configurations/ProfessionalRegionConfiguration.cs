using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class ProfessionalRegionConfiguration : IEntityTypeConfiguration<ProfessionalRegion>
{
    public void Configure(EntityTypeBuilder<ProfessionalRegion> builder)
    {
        builder.ToTable(TableNames.ProfessionalRegions);

        builder.Property(x => x.ConfidenceLevel)
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.ProfessionalId, x.RegionId })
            .IsUnique();

        builder.HasOne(x => x.Professional)
            .WithMany(x => x.ProfessionalRegions)
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Region)
            .WithMany(x => x.ProfessionalRegions)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
