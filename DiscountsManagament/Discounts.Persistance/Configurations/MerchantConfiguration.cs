using Discounts.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounts.Persistance.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.LogoUrl)
            .HasMaxLength(500);

        builder.Property(m => m.ContactEmail)
            .HasMaxLength(256);

        builder.Property(m => m.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(m => m.Address)
            .HasMaxLength(500);

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.HasIndex(m => m.UserId)
            .IsUnique();

        builder.HasMany(m => m.Offers)
            .WithOne(o => o.Merchant)
            .HasForeignKey(o => o.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}