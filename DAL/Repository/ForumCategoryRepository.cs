using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class ForumCategoryRepository : IForumCategoryRepository
    {
        private readonly VerdantTechDbContext _context;

        public ForumCategoryRepository(VerdantTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumCategory>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            return await _context.ForumCategories
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<ForumCategory?> GetByIdAsync(
            ulong id,
            CancellationToken cancellationToken = default)
        {
            return await _context.ForumCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task CreateAsync(
            ForumCategory entity,
            CancellationToken cancellationToken = default)
        {
            await _context.ForumCategories.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            ForumCategory entity,
            CancellationToken cancellationToken = default)
        {
            _context.ForumCategories.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            ulong id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.ForumCategories
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity is null) return;

            _context.ForumCategories.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
