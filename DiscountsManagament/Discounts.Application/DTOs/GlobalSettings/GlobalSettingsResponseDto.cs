namespace Discounts.Application.DTOs.GlobalSettings;

public class GlobalSettingsResponseDto
{
    public int Id { get; set; }
    public int ReservationDurationMinutes { get; set; }
    public int MerchantEditWindowHours { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedByAdminEmail { get; set; }
}