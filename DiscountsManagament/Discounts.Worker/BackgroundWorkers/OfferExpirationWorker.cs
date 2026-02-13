// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.IRepositories;
using Discounts.Domain.Enums;

namespace Discounts.Worker.BackgroundWorkers
{
    public class OfferExpirationWorker : BackgroundService
    {
        private readonly TimeSpan
            _interval = TimeSpan.FromHours(1); // since offer has hours until experation i used this instead of schedule

        private readonly ILogger<OfferExpirationWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public OfferExpirationWorker(
            ILogger<OfferExpirationWorker> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OfferExpirationWorker started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredOffersAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing expired offers");
                }

                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }

            _logger.LogInformation("OfferExpirationWorker stopped at {Time}", DateTime.UtcNow);
        }

        private async Task ProcessExpiredOffersAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expiredOffers =
                    await unitOfWork.Offers.GetExpiredOffersAsync(cancellationToken).ConfigureAwait(false);

                var expiredList = expiredOffers.ToList();

                if (!expiredList.Any())
                {
                    _logger.LogInformation("No expired offers found at {Time}", DateTime.UtcNow);
                    return;
                }

                _logger.LogInformation("Found {Count} expired offers to process", expiredList.Count);

                foreach (var offer in expiredList)
                {
                    try
                    {
                        offer.Status = OfferStatus.Expired;
                        unitOfWork.Offers.Update(offer);

                        _logger.LogInformation(
                            "Marked offer {OfferId} ({Title}) as Expired - EndDate was {EndDate}",
                            offer.Id,
                            offer.Title,
                            offer.EndDate);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing offer {OfferId}", offer.Id);
                    }
                }

                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Successfully processed {Count} expired offers at {Time}",
                    expiredList.Count,
                    DateTime.UtcNow);
            }
        }
    }
}
