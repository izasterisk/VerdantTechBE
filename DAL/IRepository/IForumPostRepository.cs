using DAL.Data.Models;
using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IForumPostRepository
    {
        // ---------------- CRUD ----------------
        Task<IEnumerable<ForumPost>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<ForumPost>> GetAllByCategoryIdAsync(ulong categoryId, int page, int pageSize, CancellationToken ct = default);
        Task<ForumPost?> GetDetailAsync(ulong id, CancellationToken ct = default);
        Task CreateAsync( ForumPost post, IEnumerable<MediaLink>? addForumPostImg, CancellationToken ct = default);
        Task UpdateAsync( ForumPost post, IEnumerable<MediaLink>? addForumPostImg, IEnumerable<string>? removeForumPostImg, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);

        // ---------------- PIN + STATUS ----------------
        Task PinAsync(ulong id, bool isPinned, CancellationToken ct = default);
        Task ChangeStatusAsync(ulong id, ForumPostStatus status, CancellationToken ct = default);

        // ---------------- COUNTERS ----------------
        Task IncrementViewAsync(ulong id, CancellationToken ct = default);
        Task IncrementLikeAsync(ulong id, CancellationToken ct = default);
        Task IncrementDislikeAsync(ulong id, CancellationToken ct = default);

        // ---------------- POST WITH COMMENTS ----------------
        Task<ForumPost?> GetPostWithCommentsAsync(ulong id, CancellationToken ct = default);
    }
}
