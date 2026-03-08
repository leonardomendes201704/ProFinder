using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable(TableNames.Professionals);

        builder.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.BusinessName)
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.WhatsApp)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(150);

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(30);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.Website)
            .HasMaxLength(200);

        builder.Property(x => x.Instagram)
            .HasMaxLength(100);

        builder.HasIndex(x => x.FullName);
        builder.HasIndex(x => x.Phone);
        builder.HasIndex(x => x.WhatsApp);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.IsActive);

        builder.HasOne(x => x.Profession)
            .WithMany(x => x.Professionals)
            .HasForeignKey(x => x.ProfessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Source)
            .WithMany(x => x.Professionals)
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Status)
            .WithMany(x => x.Professionals)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
