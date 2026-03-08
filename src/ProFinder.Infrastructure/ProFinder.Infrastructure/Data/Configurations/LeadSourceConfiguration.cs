using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class LeadSourceConfiguration : IEntityTypeConfiguration<LeadSource>
{
    public void Configure(EntityTypeBuilder<LeadSource> builder)
    {
        builder.ToTable(TableNames.LeadSources);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Url)
            .HasMaxLength(200);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
