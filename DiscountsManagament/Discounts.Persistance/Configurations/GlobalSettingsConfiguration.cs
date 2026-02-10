using Discounts.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounts.Persistance.Configurations;

public class GlobalSettingsConfiguration : IEntityTypeConfiguration<GlobalSettings>
{
    public void Configure(EntityTypeBuilder<GlobalSettings> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ReservationDurationMinutes)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(s => s.MerchantEditWindowHours)
            .IsRequired()
            .HasDefaultValue(24);

        builder.Property(s => s.UpdatedByAdminId)
            .HasMaxLength(450);

        builder.HasOne(s => s.UpdatedByAdmin)
            .WithMany()
            .HasForeignKey(s => s.UpdatedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}