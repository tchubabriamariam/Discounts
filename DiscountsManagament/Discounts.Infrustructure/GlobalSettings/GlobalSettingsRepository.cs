using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.GlobalSettings;

public class GlobalSettingsRepository : IGlobalSettingsRepository
{
    private readonly ApplicationDbContext _context;
    private IGlobalSettingsRepository _globalSettingsRepositoryImplementation;

    public GlobalSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entity.GlobalSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.GlobalSettings.FirstAsync(cancellationToken);
    }

    public async Task UpdateAsync(Domain.Entity.GlobalSettings settings, CancellationToken cancellationToken = default)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _context.GlobalSettings.Update(settings);
        await _context.SaveChangesAsync(cancellationToken);
    }
}