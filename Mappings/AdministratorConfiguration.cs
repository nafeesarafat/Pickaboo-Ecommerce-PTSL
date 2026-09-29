using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Api.Mappings;

public class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> builder)
    {
        builder.ToTable("Administrator");
        builder.HasKey(a => a.AdministratorId);

        builder.Property(a => a.FullName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(256);
        builder.Property(a => a.PhoneNumber).HasMaxLength(20);
        builder.Property(a => a.Username).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PasswordHash).IsRequired().HasMaxLength(256);
        // RoleId is stored as a plain column: the role table belongs to a later roadmap phase,
        // so no foreign key is configured here yet.
        builder.Property(a => a.RoleId).IsRequired();
        builder.Property(a => a.AccountStatus).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
        builder.Property(a => a.CreatedDate).IsRequired();

        builder.HasIndex(a => a.Email).IsUnique();
        builder.HasIndex(a => a.Username).IsUnique();
        builder.HasIndex(a => a.RoleId);
    }
}
