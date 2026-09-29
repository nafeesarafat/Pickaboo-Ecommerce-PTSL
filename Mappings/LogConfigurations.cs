using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");
        builder.HasKey(a => a.AuditLogId);

        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.Timestamp).IsRequired();
        builder.Property(a => a.Status).IsRequired().HasMaxLength(50);

        // Audit records must survive: deleting an administrator is blocked while logs exist.
        builder.HasOne(a => a.Administrator)
            .WithMany(ad => ad.AuditLogs)
            .HasForeignKey(a => a.AdministratorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(a => a.AdministratorId);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.Action);
    }
}

public class LoginLogConfiguration : IEntityTypeConfiguration<LoginLog>
{
    public void Configure(EntityTypeBuilder<LoginLog> builder)
    {
        builder.ToTable("LoginLog");
        builder.HasKey(l => l.LogId);

        builder.Property(l => l.AdministratorName).HasMaxLength(200);
        builder.Property(l => l.LoginDateTime).IsRequired();
        builder.Property(l => l.IpAddress).HasMaxLength(45);
        builder.Property(l => l.Status).IsRequired().HasMaxLength(50);

        builder.HasOne(l => l.Administrator)
            .WithMany(a => a.LoginLogs)
            .HasForeignKey(l => l.AdministratorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(l => l.AdministratorId);
        builder.HasIndex(l => l.LoginDateTime);
        builder.HasIndex(l => l.Status);
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLog");
        builder.HasKey(a => a.LogId);

        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.Timestamp).IsRequired();

        // Every reference is optional and detaches (SetNull) when the target row is removed,
        // because a log row must never be deleted and must never block a business delete.
        builder.HasOne(a => a.Order)
            .WithMany()
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Product)
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.OrderId);
        builder.HasIndex(a => a.ProductId);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Timestamp);
    }
}

public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.ToTable("SystemLog");
        builder.HasKey(s => s.LogId);

        builder.Property(s => s.Level).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Message).IsRequired().HasMaxLength(4000);
        builder.Property(s => s.Timestamp).IsRequired();

        builder.HasIndex(s => s.Timestamp);
        builder.HasIndex(s => s.Level);
    }
}
