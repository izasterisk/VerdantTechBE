using BLL.DTO.ForumCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IForumCategoryService
    {
        Task<IEnumerable<ForumCategoryResponseDto>> GetAllAsync( int page, int pageSize, CancellationToken cancellationToken = default);
        Task<ForumCategoryResponseDto?> GetByIdAsync( ulong id, CancellationToken cancellationToken = default);
        Task CreateAsync( ForumCategoryCreateDTO dto, CancellationToken cancellationToken = default);
        Task UpdateAsync( ulong id, ForumCategoryUpdateDTO dto, CancellationToken cancellationToken = default);
        Task DeleteAsync( ulong id, CancellationToken cancellationToken = default);
    }
}
