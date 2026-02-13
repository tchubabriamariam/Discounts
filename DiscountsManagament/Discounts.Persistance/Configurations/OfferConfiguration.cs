// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounts.Persistance.Configurations
{
    public class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(o => o.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(o => o.ImageUrl)
                .HasMaxLength(500);

            builder.Property(o => o.OriginalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.DiscountedPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.TotalCoupons)
                .IsRequired();

            builder.Property(o => o.RemainingCoupons)
                .IsRequired();

            builder.Property(o => o.Status)
                .IsRequired()
                .HasDefaultValue(OfferStatus.Pending);

            builder.Property(o => o.RejectionReason)
                .HasMaxLength(500);

            builder.Property(o => o.ApprovedByAdminId)
                .HasMaxLength(450);

            // DiscountPercentage is already computed 
            builder.Ignore(o => o.DiscountPercentage);

            builder.HasOne(o => o.Category)
                .WithMany(c => c.Offers)
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.ApprovedByAdmin)
                .WithMany()
                .HasForeignKey(o => o.ApprovedByAdminId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(o => o.Coupons)
                .WithOne(c => c.Offer)
                .HasForeignKey(c => c.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Reservations)
                .WithOne(r => r.Offer)
                .HasForeignKey(r => r.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(o => !o.IsDeleted);
        }
    }
}
