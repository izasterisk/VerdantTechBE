using DAL.Data.Models;
using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IForumCommentRepository
    {
        Task<IEnumerable<ForumComment>> GetCommentsByPostIdAsync( ulong postId, int page, int pageSize, CancellationToken ct = default);
        Task<ForumComment?> GetDetailAsync(ulong id, CancellationToken ct = default);
        Task CreateAsync(ForumComment comment, CancellationToken ct = default);
        Task UpdateAsync(ForumComment comment, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
        Task ChangeStatusAsync(ulong id, ForumCommentStatus status, CancellationToken ct = default);
        Task IncrementLikeAsync(ulong id, CancellationToken ct = default);
        Task IncrementDislikeAsync(ulong id, CancellationToken ct = default);
        Task<ForumComment?> GetCommentWithRepliesAsync(ulong id, CancellationToken ct = default);
        Task<List<ForumComment>> GetAllRepliesRecursiveAsync(ulong parentId, CancellationToken ct = default);
    }
}
