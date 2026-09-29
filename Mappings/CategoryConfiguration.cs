using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category");
        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.CategoryName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.ImageUrl).HasMaxLength(ColumnConventions.MaxUrlLength);
        builder.Property(c => c.IsActive).HasDefaultValue(true);

        builder.HasIndex(c => c.CategoryName).IsUnique();
        builder.HasIndex(c => c.IsActive);
    }
}
