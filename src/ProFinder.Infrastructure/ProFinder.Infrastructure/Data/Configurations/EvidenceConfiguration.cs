using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
{
    public void Configure(EntityTypeBuilder<Evidence> builder)
    {
        builder.ToTable(TableNames.Evidences);

        builder.Property(x => x.FieldName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.FieldValue)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.SourceUrl)
            .HasMaxLength(200);

        builder.HasOne(x => x.Professional)
            .WithMany(x => x.Evidences)
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
