using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Review");
        builder.HasKey(r => r.ReviewId);

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.Property(r => r.RejectionReason).HasMaxLength(500);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(r => r.CreatedDate).IsRequired();

        // Reviews belong to a product, so removing the product removes its reviews.
        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // A user cannot be deleted while they still have reviews.
        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(r => new { r.ProductId, r.UserId });
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);
    }
}
