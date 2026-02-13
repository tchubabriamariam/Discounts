// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories
{
    public class GlobalSettingsRepository : IGlobalSettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public GlobalSettingsRepository(ApplicationDbContext context) => _context = context;

        public async Task<GlobalSettings> GetAsync(CancellationToken cancellationToken = default) =>
            await _context.GlobalSettings.FirstAsync(cancellationToken).ConfigureAwait(false);

        public async Task UpdateAsync(GlobalSettings settings, CancellationToken cancellationToken = default)
        {
            settings.UpdatedAt = DateTime.UtcNow;
            _context.GlobalSettings.Update(settings);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
