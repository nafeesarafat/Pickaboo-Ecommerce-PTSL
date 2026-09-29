using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");
        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(ColumnConventions.MaxLongTextLength);
        builder.Property(p => p.Price).HasColumnType(ColumnConventions.Money);
        builder.Property(p => p.ImageUrl).HasMaxLength(ColumnConventions.MaxUrlLength);
        builder.Property(p => p.StockQuantity).IsRequired();
        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.CreatedDate).IsRequired();
        builder.Property(p => p.UpdatedDate).IsRequired();

        // Category and Brand are required. Deleting a parent that still has products is blocked
        // (Restrict) so the catalog is never silently emptied.
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Covers the most common catalog queries: browse by category, filter by brand, sort by price.
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.BrandId);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => new { p.IsActive, p.IsFeatured });
        builder.HasIndex(p => p.ProductName);
    }
}
