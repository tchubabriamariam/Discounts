// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounts.Persistance.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.UserId)
                .IsRequired();

            builder.Property(r => r.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasDefaultValue(ReservationStatus.Active);

            builder.Property(r => r.ReservedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // A user can only have one active reservation per offer at a time
            builder.HasIndex(r => new { r.UserId, r.OfferId, r.Status })
                .HasFilter("[Status] = 0")
                .IsUnique();

            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
