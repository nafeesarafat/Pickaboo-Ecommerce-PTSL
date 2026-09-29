using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItem");
        builder.HasKey(c => c.CartItemId);

        builder.Property(c => c.Quantity).IsRequired();

        // A cart has no meaning without its user, so it is removed together with the user.
        builder.HasOne(c => c.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(c => c.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // One row per product in a user's cart.
        builder.HasIndex(c => new { c.UserId, c.ProductId }).IsUnique();
        builder.HasIndex(c => c.ProductId);
    }
}
