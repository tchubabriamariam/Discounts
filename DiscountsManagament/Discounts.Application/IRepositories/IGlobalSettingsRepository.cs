// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface IGlobalSettingsRepository
    {
        Task<GlobalSettings> GetAsync(CancellationToken cancellationToken = default);
        Task UpdateAsync(GlobalSettings settings, CancellationToken cancellationToken = default);
    }
}
