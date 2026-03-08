using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class ProviderLeadProfessionConfiguration : IEntityTypeConfiguration<ProviderLeadProfession>
{
    public void Configure(EntityTypeBuilder<ProviderLeadProfession> builder)
    {
        builder.ToTable(TableNames.ProviderLeadProfessions);

        builder.HasIndex(x => new { x.ProviderLeadId, x.ProfessionId })
            .IsUnique();

        builder.HasIndex(x => x.ProfessionId);

        builder.HasOne(x => x.ProviderLead)
            .WithMany(x => x.ProviderLeadProfessions)
            .HasForeignKey(x => x.ProviderLeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Profession)
            .WithMany(x => x.ProviderLeadProfessions)
            .HasForeignKey(x => x.ProfessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
