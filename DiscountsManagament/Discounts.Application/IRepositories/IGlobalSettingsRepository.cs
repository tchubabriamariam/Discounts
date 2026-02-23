// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface IGlobalSettingsRepository
    {
        Task<GlobalSettings> GetAsync(CancellationToken cancellationToken = default); // get gloval configuration row
        Task UpdateAsync(GlobalSettings settings, CancellationToken cancellationToken = default); // update new settings
    }
}
