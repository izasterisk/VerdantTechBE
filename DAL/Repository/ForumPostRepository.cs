using DAL.Data.Models;
using DAL.Data;
using DAL.IRepository;
using System;
using Microsoft.EntityFrameworkCore;

public class ForumPostRepository : IForumPostRepository
{
    private readonly VerdantTechDbContext _context;

    public ForumPostRepository(VerdantTechDbContext context)
    {
        _context = context;
    }

    
    public async Task<IEnumerable<ForumPost>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.ForumPosts
             .AsNoTracking()
             //.Include(x => x.MediaLinks)
             //.Include(x => x.Content)
             .OrderByDescending(x => x.CreatedAt)
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync(ct);
    }

    
    public async Task<IEnumerable<ForumPost>> GetAllByCategoryIdAsync(
        ulong categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.ForumPosts
            .AsNoTracking()
            .Where(x => x.ForumCategoryId == categoryId)
            //.Include(x => x.MediaLinks)
            //.Include(x => x.Content)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

   
    public async Task<ForumPost?> GetDetailAsync(ulong id, CancellationToken ct = default)
    {
        var post = await _context.ForumPosts
            .Include(x => x.ForumCategory)
            .Include(x => x.User)
            //.Include(x => x.Content)
            //.Include(x => x.MediaLinks)
            .Include(x => x.ForumComments).ThenInclude(c => c.User)
            .Include(x => x.ForumComments).ThenInclude(c => c.InverseParent)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (post != null)
        {
            post.MediaLinks = await _context.MediaLinks
                .Where(m => m.OwnerType == MediaOwnerType.ForumPosts && m.OwnerId == id)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);
        }

        return post;
    }

   
    public async Task CreateAsync( ForumPost post, IEnumerable<MediaLink>? addForumPostImg, CancellationToken ct = default)
    {
        await _context.ForumPosts.AddAsync(post, ct);
        await _context.SaveChangesAsync(ct);

        if (addForumPostImg != null)
        {
            foreach (var img in addForumPostImg)
                img.OwnerId = post.Id;

            await _context.MediaLinks.AddRangeAsync(addForumPostImg, ct);
            await _context.SaveChangesAsync(ct);
        }
    }

    
    public async Task UpdateAsync( ForumPost post, IEnumerable<MediaLink>? addForumPostImg, IEnumerable<string>? removeForumPostImg,CancellationToken ct = default)
    {
        _context.ForumPosts.Update(post);

        if (addForumPostImg != null)
        {
            foreach (var img in addForumPostImg)
                img.OwnerId = post.Id;

            await _context.MediaLinks.AddRangeAsync(addForumPostImg, ct);
        }

        if (removeForumPostImg != null)
        {
            var toRemove = await _context.MediaLinks
                .Where(x => removeForumPostImg.Contains(x.ImagePublicId!))
                .ToListAsync(ct);

            _context.MediaLinks.RemoveRange(toRemove);
        }

        await _context.SaveChangesAsync(ct);
    }


    
    public async Task DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var post = await _context.ForumPosts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (post != null)
        {
            _context.ForumPosts.Remove(post);

            var imgs = await _context.MediaLinks
                .Where(x => x.OwnerType == MediaOwnerType.ForumPosts && x.OwnerId == id)
                .ToListAsync(ct);

            _context.MediaLinks.RemoveRange(imgs);

            await _context.SaveChangesAsync(ct);
        }
    }

   
    public async Task PinAsync(ulong id, bool isPinned, CancellationToken ct = default)
    {
        await _context.ForumPosts
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(x => x.IsPinned, isPinned)
                 .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
            ct);
    }

   
    public async Task ChangeStatusAsync(ulong id, ForumPostStatus status, CancellationToken ct = default)
    {
        await _context.ForumPosts
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(x => x.Status, status)
                 .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
            ct);
    }

   
    public async Task IncrementViewAsync(ulong id, CancellationToken ct = default)
    {
        await _context.ForumPosts
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(x => x.ViewCount, x => x.ViewCount + 1),
            ct);
    }

    public async Task IncrementLikeAsync(ulong id, CancellationToken ct = default)
    {
        await _context.ForumPosts
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(x => x.LikeCount, x => x.LikeCount + 1),
            ct);
    }

    public async Task IncrementDislikeAsync(ulong id, CancellationToken ct = default)
    {
        await _context.ForumPosts
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(x => x.DislikeCount, x => x.DislikeCount + 1),
            ct);
    }

    
    public async Task<ForumPost?> GetPostWithCommentsAsync(ulong id, CancellationToken ct = default)
    {
        var post = await _context.ForumPosts
            //.Include(x => x.Content)
            //.Include(x => x.MediaLinks)
            .Include(x => x.ForumComments).ThenInclude(c => c.User)
            .Include(x => x.ForumComments).ThenInclude(c => c.InverseParent)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (post != null)
        {
            post.MediaLinks = await _context.MediaLinks
                .Where(m => m.OwnerType == MediaOwnerType.ForumPosts && m.OwnerId == id)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);
        }
        return post;
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct)
    {
        return await _context.ForumPosts
            .AsNoTracking()
            .AnyAsync(x => x.Slug == slug, ct);
    }
}
