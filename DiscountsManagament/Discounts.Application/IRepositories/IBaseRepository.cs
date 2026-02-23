// Copyright (C) TBC Bank. All Rights Reserved.

using System.Linq.Expressions;
using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        // read methods
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default); // get single record by pk
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default); // get all rows from table

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default); // filters by predicate and returns appropriate records

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default); // returns first one that fulfills

        // check is good but i never needed only check i always needed info after check so i use GetByIdAsync and check for null
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default); // at least one matches predicate

        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default); // counts how many matches

        // write methods
        Task AddAsync(T entity, CancellationToken cancellationToken = default); // new record to be added in table
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default); // adds many records in table
        void Update(T entity); // notifies EF that existing object has changed
        void Delete(T entity); // deletes permanently (hard delete)
        void SoftDelete(T entity); // set isDeleted flag to true
        IQueryable<T> Query(); // allows the service layer to build custom, complex queries (.Where(), .OrderBy() )
    }
}
