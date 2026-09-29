using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class StockHistoryConfiguration : IEntityTypeConfiguration<StockHistory>
{
    public void Configure(EntityTypeBuilder<StockHistory> builder)
    {
        builder.ToTable("StockHistory");
        builder.HasKey(s => s.StockHistoryId);

        builder.Property(s => s.Change).IsRequired();
        builder.Property(s => s.Reason).HasMaxLength(255);
        builder.Property(s => s.UpdatedBy).HasMaxLength(100);
        builder.Property(s => s.Timestamp).IsRequired();

        builder.HasOne(s => s.Product)
            .WithMany(p => p.StockHistoryEntries)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(s => new { s.ProductId, s.Timestamp });
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notification");
        builder.HasKey(n => n.NotificationId);

        builder.Property(n => n.Type).IsRequired().HasMaxLength(50);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
        builder.Property(n => n.IsRead).HasDefaultValue(false);
        builder.Property(n => n.CreatedDate).IsRequired();

        builder.HasOne(n => n.Order)
            .WithMany(o => o.Notifications)
            .HasForeignKey(n => n.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.Product)
            .WithMany(p => p.Notifications)
            .HasForeignKey(n => n.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.OrderId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}

public class NotificationSettingsConfiguration : IEntityTypeConfiguration<NotificationSettings>
{
    public void Configure(EntityTypeBuilder<NotificationSettings> builder)
    {
        builder.ToTable("NotificationSettings");
        builder.HasKey(s => s.SettingsId);

        builder.Property(s => s.OrderAlerts).HasDefaultValue(true);
        builder.Property(s => s.StockAlerts).HasDefaultValue(true);
        builder.Property(s => s.PromotionalAlerts).HasDefaultValue(true);

        // UserId is optional so a store-wide default row can exist without a user.
        builder.HasOne(s => s.User)
            .WithOne(u => u.NotificationSettings)
            .HasForeignKey<NotificationSettings>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId).IsUnique().HasFilter(null);
    }
}

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Session");
        builder.HasKey(s => s.SessionId);

        builder.Property(s => s.Device).HasMaxLength(200);
        builder.Property(s => s.LoginDateTime).IsRequired();
        builder.Property(s => s.ExpiresAtUtc).IsRequired();
        builder.Property(s => s.IsActive).HasDefaultValue(true);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.LoginDateTime);
    }
}
