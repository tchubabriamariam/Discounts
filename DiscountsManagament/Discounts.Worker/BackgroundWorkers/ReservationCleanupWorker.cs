using Discounts.Application.IRepositories;
using Discounts.Domain.Enums;

namespace Discounts.Worker.BackgroundWorkers;

public class ReservationCleanupWorker : BackgroundService
{
    private readonly ILogger<ReservationCleanupWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // did not use schedule because this is much simpler for this case

    public ReservationCleanupWorker(
        ILogger<ReservationCleanupWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReservationCleanupWorker started at {Time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredReservationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired reservations");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("ReservationCleanupWorker stopped at {Time}", DateTime.UtcNow);
    }

    private async Task ProcessExpiredReservationsAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateAsyncScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expiredReservations = await unitOfWork.Reservations.GetExpiredReservationsAsync(cancellationToken);

            var expiredList = expiredReservations.ToList();

            if (!expiredList.Any())
            {
                _logger.LogInformation("No expired reservations found at {Time}", DateTime.UtcNow);
                return;
            }

            _logger.LogInformation("Found {Count} expired reservations to process", expiredList.Count);

            foreach (var reservation in expiredList)
            {
                try
                {
                    reservation.Status = ReservationStatus.Expired;

                    if (reservation.Offer is not null)
                    {
                        reservation.Offer.RemainingCoupons += reservation.Quantity;
                        unitOfWork.Offers.Update(reservation.Offer);
                    }

                    unitOfWork.Reservations.Update(reservation);

                    _logger.LogInformation(
                        "Expired reservation {ReservationId}: restored {Quantity} coupons to offer {OfferId}",
                        reservation.Id,
                        reservation.Quantity,
                        reservation.OfferId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing reservation {ReservationId}", reservation.Id);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully processed {Count} expired reservations at {Time}",
                expiredList.Count,
                DateTime.UtcNow);
        }
    }
}






















































