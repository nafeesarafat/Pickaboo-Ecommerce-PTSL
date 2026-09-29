using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brand");
        builder.HasKey(b => b.BrandId);

        builder.Property(b => b.BrandName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Description).HasMaxLength(1000);
        builder.Property(b => b.ImageUrl).HasMaxLength(ColumnConventions.MaxUrlLength);
        builder.Property(b => b.IsActive).HasDefaultValue(true);
        builder.Property(b => b.UpdatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(b => b.BrandName).IsUnique();
        builder.HasIndex(b => b.IsActive);
    }
}
