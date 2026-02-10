using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounts.Persistance.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.PricePaid)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasDefaultValue(CouponStatus.Active);

        builder.Property(c => c.PurchasedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}