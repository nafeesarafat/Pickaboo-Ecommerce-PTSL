using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItem");
        builder.HasKey(oi => oi.OrderItemId);

        // ProductName, Price and TotalAmount are snapshots taken at purchase time.
        builder.Property(oi => oi.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(oi => oi.Price).HasColumnType(ColumnConventions.Money);
        builder.Property(oi => oi.Quantity).IsRequired();
        builder.Property(oi => oi.TotalAmount).HasColumnType(ColumnConventions.Money);

        // An order item is meaningless without its order.
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // A product that already appears in an order can never be deleted.
        builder.HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);
    }
}
