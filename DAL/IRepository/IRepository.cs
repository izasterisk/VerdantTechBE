using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<T>> GetAllByFilterAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllWithRelationsAsync(Func<IQueryable<T>, IQueryable<T>>? includeFunc = null, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllWithRelationsByFilterAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false, Func<IQueryable<T>, IQueryable<T>>? includeFunc = null, CancellationToken cancellationToken = default);
        Task<T?> GetWithRelationsAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false, Func<IQueryable<T>, IQueryable<T>>? includeFunc = null, CancellationToken cancellationToken = default);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false, CancellationToken cancellationToken = default);
        //Task<T> GetByNameAsync(Expression<Func<T, bool>> filter);
        Task<T> CreateAsync(T dbRecord, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T dbRecord, CancellationToken cancellationToken = default);
        Task<List<T>> BatchUpdateAsync(List<T> dbRecords, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(T dbRecord, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
        Task<(List<T> items, int totalCount)> GetPaginatedAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, bool useNoTracking = false, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, CancellationToken cancellationToken = default);
        Task<(List<T> items, int totalCount)> GetPaginatedWithRelationsAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, bool useNoTracking = false, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>? includeFunc = null, CancellationToken cancellationToken = default);
    }
}
