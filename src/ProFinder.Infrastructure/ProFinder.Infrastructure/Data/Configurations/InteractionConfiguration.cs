using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class InteractionConfiguration : IEntityTypeConfiguration<Interaction>
{
    public void Configure(EntityTypeBuilder<Interaction> builder)
    {
        builder.ToTable(TableNames.Interactions);

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.InteractionType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => new { x.ProfessionalId, x.InteractionDate });

        builder.HasOne(x => x.Professional)
            .WithMany(x => x.Interactions)
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
