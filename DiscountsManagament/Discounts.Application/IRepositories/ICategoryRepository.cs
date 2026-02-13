// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    }
}
