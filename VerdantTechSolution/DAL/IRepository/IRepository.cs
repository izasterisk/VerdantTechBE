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
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllByFilterAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false);
        Task<List<T>> GetAllWithRelationsAsync(Func<IQueryable<T>, IQueryable<T>> includeFunc = null);
        Task<List<T>> GetAllWithRelationsByFilterAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false, Func<IQueryable<T>, IQueryable<T>> includeFunc = null);
        Task<T?> GetWithRelationsAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false, Func<IQueryable<T>, IQueryable<T>> includeFunc = null);
        Task<T> GetAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false);
        //Task<T> GetByNameAsync(Expression<Func<T, bool>> filter);
        Task<T> CreateAsync(T dbRecord);
        Task<T> UpdateAsync(T dbRecord);
        Task<bool> DeleteAsync(T dbRecord);
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
    }
}
