using DAL.Data.Models;
using DAL.Data;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public class ForumCommentRepository : IForumCommentRepository
    {
        private readonly VerdantTechDbContext _context;

        public ForumCommentRepository(VerdantTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumComment>> GetCommentsByPostIdAsync(
            ulong postId, int page, int pageSize, CancellationToken ct = default)
        {
            return await _context.ForumComments
                .Where(c => c.ForumPostId == postId && c.ParentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.InverseParent)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<ForumComment?> GetDetailAsync(ulong id, CancellationToken ct = default)
        {
            return await _context.ForumComments
                .Include(c => c.InverseParent)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task CreateAsync(ForumComment comment, CancellationToken ct = default)
        {
            await _context.ForumComments.AddAsync(comment, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(ForumComment comment, CancellationToken ct = default)
        {
            _context.ForumComments.Update(comment);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var c = await _context.ForumComments.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c != null)
            {
                _context.ForumComments.Remove(c);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task ChangeStatusAsync(ulong id, ForumCommentStatus status, CancellationToken ct = default)
        {
            await _context.ForumComments
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, status), ct);
        }

        public async Task IncrementLikeAsync(ulong id, CancellationToken ct = default)
        {
            await _context.ForumComments
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.LikeCount, x => x.LikeCount + 1), ct);
        }

        public async Task IncrementDislikeAsync(ulong id, CancellationToken ct = default)
        {
            await _context.ForumComments
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.DislikeCount, x => x.DislikeCount + 1), ct);
        }

        public async Task<ForumComment?> GetCommentWithRepliesAsync(ulong id, CancellationToken ct = default)
        {
            return await _context.ForumComments
                .Include(x => x.InverseParent)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<List<ForumComment>> GetAllRepliesRecursiveAsync(
            ulong parentId,
            CancellationToken ct = default)
        {
            var result = new List<ForumComment>();

            var directChildren = await _context.ForumComments
                .Where(c => c.ParentId == parentId)
                .ToListAsync(ct);

            foreach (var child in directChildren)
            {
                result.Add(child);
                var sub = await GetAllRepliesRecursiveAsync(child.Id, ct);
                result.AddRange(sub);
            }

            return result;
        }
    }
}
