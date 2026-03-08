using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class LeadStatusConfiguration : IEntityTypeConfiguration<LeadStatus>
{
    public void Configure(EntityTypeBuilder<LeadStatus> builder)
    {
        builder.ToTable(TableNames.LeadStatuses);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
