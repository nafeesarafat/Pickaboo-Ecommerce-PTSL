using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order");
        builder.HasKey(o => o.OrderId);

        builder.Property(o => o.TotalAmount).HasColumnType(ColumnConventions.Money);
        builder.Property(o => o.SubTotal).HasColumnType(ColumnConventions.Money);
        builder.Property(o => o.ShippingFee).HasColumnType(ColumnConventions.Money);
        builder.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(500);
        builder.Property(o => o.City).HasMaxLength(100);
        builder.Property(o => o.Country).HasMaxLength(100);
        builder.Property(o => o.PhoneNumber).HasMaxLength(20);
        builder.Property(o => o.OrderStatus).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
        builder.Property(o => o.DeliveryStatus).HasMaxLength(50);
        builder.Property(o => o.OrderDate).IsRequired();
        builder.Property(o => o.UpdatedDate);

        // Orders are financial history: deleting a user must not delete their orders.
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.OrderStatus);
    }
}
