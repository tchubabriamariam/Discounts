namespace Discounts.Infrustructure.GlobalSettings;

public interface IGlobalSettingsRepository
{
    Task<Domain.Entity.GlobalSettings> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entity.GlobalSettings settings, CancellationToken cancellationToken = default);
}