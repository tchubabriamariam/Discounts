namespace Discounts.Domain.Entity;

public class GlobalSettings
{
    public int Id { get; set; }

    // How long (minutes) a reservation is held before being auto-cancelled
    public int ReservationDurationMinutes { get; set; } = 30;

    // How long (hours) a merchant can edit their offer after creation
    public int MerchantEditWindowHours { get; set; } = 24;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByAdminId { get; set; }

    // Navigation
    public ApplicationUser? UpdatedByAdmin { get; set; }
}