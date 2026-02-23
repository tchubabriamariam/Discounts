// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default); // returns active categories

        // this is for redundant names we don't want 2 categories with same name, TBC and tbc is the same
        Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    }
}
