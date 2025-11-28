using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly VerdantTechDbContext _db;
        public ProductRepository(VerdantTechDbContext db) => _db = db;

        // ========== READS (phân trang) ==========
        public async Task<(IReadOnlyList<Product> Items, int Total)> GetAllProductAsync(
            int page, int pageSize, CancellationToken ct = default)
        {
            NormalizePaging(ref page, ref pageSize);

            var query = _db.Products.AsNoTracking();
            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            await LoadMediaAsync(items, ct);

            return (items, total);
        }

        public async Task<Product?> GetProductByIdAsync(
            ulong id, bool useNoTracking = true, CancellationToken ct = default)
        {
            var query = useNoTracking
                ? _db.Products.AsNoTracking()
                : _db.Products.AsQueryable();

            query = query.Include(x => x.Category);

            var entity = await query.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return null;

            await LoadMediaAsync(new List<Product> { entity }, ct);
            return entity;
        }

        public async Task<(IReadOnlyList<Product> Items, int Total)> GetAllProductByCategoryIdAsync(
            ulong categoryId, int page, int pageSize, CancellationToken ct = default)
        {
            NormalizePaging(ref page, ref pageSize);

            var query = _db.Products
                .AsNoTracking()
                .Where(x => x.CategoryId == categoryId);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            await LoadMediaAsync(items, ct);
            return (items, total);
        }

        public async Task<(IReadOnlyList<Product> Items, int Total)> GetAllProductByVendorIdAsync(
            ulong vendorId, int page, int pageSize, CancellationToken ct = default)
        {
            NormalizePaging(ref page, ref pageSize);

            var query = _db.Products
                .AsNoTracking()
                .Where(x => x.VendorId == vendorId);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            await LoadMediaAsync(items, ct);
            return (items, total);
        }

        // ========== UPDATE ==========
        public async Task<Product> UpdateProductAsync(
            Product product, CancellationToken ct = default)
        {
            var existing = await _db.Products.FirstOrDefaultAsync(x => x.Id == product.Id, ct)
                           ?? throw new KeyNotFoundException("Product not found.");

            // cập nhật các field cơ bản
            _db.Entry(existing).CurrentValues.SetValues(product);
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            // nạp media nếu Service cần map ảnh ra DTO
            await LoadMediaAsync(new List<Product> { existing }, ct);
            return existing;
        }

        // ========== UPDATE EMISSION / COMMISSION ==========
        public async Task<bool> UpdateEmissionAsync(
            ulong productId, decimal CommissionRate, CancellationToken ct = default)
        {
            var entity = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId, ct);
            if (entity is null) return false;

            // nếu tên property khác (ví dụ EmissionRate), đổi dòng dưới cho đúng:
            entity.CommissionRate = CommissionRate; // TODO: đổi thành EmissionRate nếu model của bạn dùng tên đó
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        // ========== DELETE ==========
        public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return false;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // Xoá media liên quan (ảnh sản phẩm) trong MediaLink
            var medias = await _db.MediaLinks
                .Where(m => m.OwnerType == MediaOwnerType.Products && m.OwnerId == id)
                .ToListAsync(ct);

            if (medias.Count > 0)
                _db.MediaLinks.RemoveRange(medias);

            _db.Products.Remove(entity);

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return true;
        }

        public async Task UpdateStockAsync(ulong productId, int newQuantity, CancellationToken ct = default)
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId, ct);
            if (product == null) return;

            product.StockQuantity = newQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }


        private static void NormalizePaging(ref int page, ref int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
        }

        private async Task LoadMediaAsync(IReadOnlyList<Product> products, CancellationToken ct)
        {
            if (products.Count == 0) return;

            var ids = products.Select(p => p.Id).ToList();

            // Lấy ảnh thuộc Product trong MediaLink
            var medias = await _db.MediaLinks
                .AsNoTracking()
                .Where(m => m.OwnerType == MediaOwnerType.Products && ids.Contains(m.OwnerId))
                .OrderBy(m => m.OwnerId)
                .ThenBy(m => m.SortOrder)
                .ToListAsync(ct);

            
        }
    }
}
