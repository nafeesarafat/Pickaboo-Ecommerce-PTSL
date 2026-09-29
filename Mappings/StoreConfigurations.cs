using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethod");
        builder.HasKey(p => p.PaymentMethodId);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(50);
        builder.Property(p => p.IsActive).HasDefaultValue(true);

        builder.HasIndex(p => p.Name).IsUnique();
    }
}

public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.ToTable("ShippingMethod");
        builder.HasKey(s => s.ShippingMethodId);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.Property(s => s.IsActive).HasDefaultValue(true);

        builder.HasIndex(s => s.Name).IsUnique();
    }
}

public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.ToTable("ShippingZone");
        builder.HasKey(z => z.ZoneId);

        builder.Property(z => z.ZoneName).IsRequired().HasMaxLength(100);
        builder.Property(z => z.Charge).HasColumnType(ColumnConventions.Money);

        builder.HasIndex(z => z.ZoneName).IsUnique();
    }
}

public class DeliveryAreaConfiguration : IEntityTypeConfiguration<DeliveryArea>
{
    public void Configure(EntityTypeBuilder<DeliveryArea> builder)
    {
        builder.ToTable("DeliveryArea");
        builder.HasKey(d => d.AreaId);

        builder.Property(d => d.AreaName).IsRequired().HasMaxLength(100);

        // Areas are meaningless without their zone.
        builder.HasOne(d => d.Zone)
            .WithMany(z => z.DeliveryAreas)
            .HasForeignKey(d => d.ZoneId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(d => new { d.ZoneId, d.AreaName }).IsUnique();
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupon");
        builder.HasKey(c => c.CouponId);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.DiscountAmount).HasColumnType(ColumnConventions.Money);
        builder.Property(c => c.UsageCount).IsRequired();
        builder.Property(c => c.ExpiryDate).IsRequired();

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.ExpiryDate);
    }
}

public class StoreSettingsConfiguration : IEntityTypeConfiguration<StoreSettings>
{
    public void Configure(EntityTypeBuilder<StoreSettings> builder)
    {
        builder.ToTable("StoreSettings");
        builder.HasKey(s => s.SettingsId);

        builder.Property(s => s.StoreKey).IsRequired().HasMaxLength(30).HasDefaultValue("default");
        builder.Property(s => s.StoreName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.LogoUrl).HasMaxLength(ColumnConventions.MaxUrlLength);
        builder.Property(s => s.Timezone).HasMaxLength(50);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.Email).HasMaxLength(256);
        builder.Property(s => s.CurrencyCode).HasMaxLength(5);
        builder.Property(s => s.Symbol).HasMaxLength(10);
        builder.Property(s => s.DefaultShippingFee).HasColumnType(ColumnConventions.Money);
        builder.Property(s => s.SmtpHost).HasMaxLength(255);
        builder.Property(s => s.SmtpPort);

        builder.HasIndex(s => s.StoreName).IsUnique();
        builder.HasIndex(s => s.StoreKey).IsUnique();
    }
}
