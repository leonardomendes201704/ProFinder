using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable(TableNames.Regions);

        builder.Property(x => x.State)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Neighborhood)
            .HasMaxLength(100);

        builder.Property(x => x.ZipCode)
            .HasMaxLength(20);

        builder.Property(x => x.Zone)
            .HasMaxLength(50);

        builder.Property(x => x.Latitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.Longitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.RadiusKm)
            .HasColumnType("decimal(8,2)");

        builder.HasIndex(x => new { x.State, x.City, x.Neighborhood, x.ZipCode });
    }
}
