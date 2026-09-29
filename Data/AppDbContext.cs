using System.Reflection;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Data;

/// <summary>
/// Application database context. All relationship, index and precision rules live in
/// the classes under Mappings so this file stays short and readable.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<UserInformation> UserInformation => Set<UserInformation>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Extended entities prepared for later roadmap phases
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<DeliveryArea> DeliveryAreas => Set<DeliveryArea>();
    public DbSet<StockHistory> StockHistories => Set<StockHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();
    public DbSet<StoreSettings> StoreSettings => Set<StoreSettings>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<Coupon> Coupons => Set<Coupon>();

    // Security support entity required by the authentication phase
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
