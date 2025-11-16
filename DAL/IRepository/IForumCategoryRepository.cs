using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IForumCategoryRepository
    {
        Task<IEnumerable<ForumCategory>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<ForumCategory?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);
        Task CreateAsync(ForumCategory entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(ForumCategory entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(ulong id, CancellationToken cancellationToken = default);
    }
}
