using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class UserInformationConfiguration : IEntityTypeConfiguration<UserInformation>
{
    public void Configure(EntityTypeBuilder<UserInformation> builder)
    {
        builder.ToTable("UserInformation");
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Address).HasMaxLength(255);
        builder.Property(u => u.City).HasMaxLength(100);
        builder.Property(u => u.Country).HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.RegistrationDate).IsRequired();
        builder.Property(u => u.LastLoginDateTime);
        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("Customer");
        builder.Property(u => u.IsEmailVerified).HasDefaultValue(false);
        builder.Property(u => u.EmailVerificationTokenHash).HasMaxLength(128);
        builder.Property(u => u.EmailVerificationTokenExpiresAt);
        builder.Property(u => u.TwoFactorSecretProtected).HasMaxLength(2048);
        builder.Property(u => u.TwoFactorQrCodeUrlProtected).HasMaxLength(2048);
        builder.Property(u => u.IsTwoFactorEnabled).HasDefaultValue(false);

        // Email and UserName are the natural lookup keys for authentication.
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.IsActive);
    }
}
