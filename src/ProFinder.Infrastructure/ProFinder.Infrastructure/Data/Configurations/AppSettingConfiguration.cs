using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProFinder.Domain.Entities;

namespace ProFinder.Infrastructure.Data.Configurations;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable(TableNames.AppSettings);

        builder.Property(x => x.Key)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.DataType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.DefaultValue)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.HasIndex(x => new { x.Category, x.DisplayOrder });
    }
}
