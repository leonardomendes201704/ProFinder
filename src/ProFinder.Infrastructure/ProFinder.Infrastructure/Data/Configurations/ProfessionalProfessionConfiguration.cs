using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class ProfessionalProfessionConfiguration : IEntityTypeConfiguration<ProfessionalProfession>
{
    public void Configure(EntityTypeBuilder<ProfessionalProfession> builder)
    {
        builder.ToTable(TableNames.ProfessionalProfessions);

        builder.HasIndex(x => new { x.ProfessionalId, x.ProfessionId })
            .IsUnique();

        builder.HasIndex(x => x.ProfessionId);

        builder.HasOne(x => x.Professional)
            .WithMany(x => x.ProfessionalProfessions)
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Profession)
            .WithMany(x => x.ProfessionalProfessions)
            .HasForeignKey(x => x.ProfessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
