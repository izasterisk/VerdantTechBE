using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public sealed class ProductReviewRepository : IProductReviewRepository
    {
        private readonly VerdantTechDbContext _db;
        private readonly IRepository<ProductReview> _reviewRepository;
        private readonly IRepository<MediaLink> _mediaLinkRepository;
        
        public ProductReviewRepository(
            VerdantTechDbContext db,
            IRepository<ProductReview> reviewRepository,
            IRepository<MediaLink> mediaLinkRepository)
        {
            _db = db;
            _reviewRepository = reviewRepository;
            _mediaLinkRepository = mediaLinkRepository;
        }

        public async Task<ProductReview?> GetProductReviewByIdAsync(
            ulong id, bool useNoTracking = true, CancellationToken ct = default)
        {
            var query = useNoTracking
                ? _db.ProductReviews.AsNoTracking()
                : _db.ProductReviews.AsQueryable();

            return await query
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<ProductReview?> GetProductReviewByProductOrderCustomerAsync(
            ulong productId, ulong orderId, ulong customerId, bool useNoTracking = true, CancellationToken ct = default)
        {
            var query = useNoTracking
                ? _db.ProductReviews.AsNoTracking()
                : _db.ProductReviews.AsQueryable();

            return await query
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(x => 
                    x.ProductId == productId && 
                    x.OrderId == orderId && 
                    x.CustomerId == customerId, ct);
        }

        public async Task<(IReadOnlyList<ProductReview> Items, int Total)> GetProductReviewsByProductIdAsync(
            ulong productId, int page, int pageSize, CancellationToken ct = default)
        {
            NormalizePaging(ref page, ref pageSize);

            var query = _db.ProductReviews
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .Include(r => r.Customer)
                .OrderByDescending(x => x.CreatedAt);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<ProductReview> Items, int Total)> GetProductReviewsByOrderIdAsync(
            ulong orderId, int page, int pageSize, CancellationToken ct = default)
        {
            NormalizePaging(ref page, ref pageSize);

            var query = _db.ProductReviews
                .AsNoTracking()
                .Where(x => x.OrderId == orderId)
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .OrderByDescending(x => x.CreatedAt);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<ProductReview> Items, int Total)> GetProductReviewsByCustomerIdAsync(
            ulong customerId, int page, int pageSize, CancellationToken ct = default)
        {
            NormalizePaging(ref page, ref pageSize);

            var query = _db.ProductReviews
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId)
                .Include(r => r.Product)
                .Include(r => r.Order)
                .OrderByDescending(x => x.CreatedAt);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<ProductReview> CreateProductReviewWithTransactionAsync(
            ProductReview review, List<MediaLink>? mediaLinks, CancellationToken ct = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                review.CreatedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;
                var createdReview = await _reviewRepository.CreateAsync(review, ct);

                if (mediaLinks != null && mediaLinks.Count > 0)
                {
                    int sort = 1;
                    foreach (var mediaLink in mediaLinks)
                    {
                        mediaLink.OwnerType = MediaOwnerType.ProductReviews;
                        mediaLink.OwnerId = createdReview.Id;
                        mediaLink.Purpose = MediaPurpose.None;
                        mediaLink.SortOrder = sort++;
                        mediaLink.CreatedAt = DateTime.UtcNow;
                        mediaLink.UpdatedAt = DateTime.UtcNow;
                        await _mediaLinkRepository.CreateAsync(mediaLink, ct);
                    }
                }

                await transaction.CommitAsync(ct);

                // Reload with relations
                return await GetProductReviewByIdAsync(createdReview.Id, useNoTracking: false, ct) 
                       ?? createdReview;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<ProductReview> UpdateProductReviewAsync(
            ProductReview review, CancellationToken ct = default)
        {
            var existing = await _db.ProductReviews
                .FirstOrDefaultAsync(x => x.Id == review.Id, ct)
                ?? throw new KeyNotFoundException("ProductReview not found.");

            existing.Rating = review.Rating;
            existing.Comment = review.Comment;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            // Reload with relations
            return await GetProductReviewByIdAsync(existing.Id, useNoTracking: false, ct) 
                   ?? existing;
        }

        public async Task<bool> DeleteProductReviewAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _db.ProductReviews
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            
            if (entity is null) return false;

            _db.ProductReviews.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<decimal> CalculateProductRatingAverageAsync(
            ulong productId, CancellationToken ct = default)
        {
            var reviews = await _db.ProductReviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .Select(r => r.Rating)
                .ToListAsync(ct);

            if (reviews.Count == 0) return 0.00m;

            var average = reviews.Average(r => (decimal)r);
            return Math.Round(average, 2);
        }

        public async Task<List<MediaLink>> GetAllImagesByReviewIdAsync(
            ulong reviewId, CancellationToken ct = default)
        {
            var mediaLinks = await _mediaLinkRepository.GetAllByFilterAsync(
                ml => ml.OwnerType == MediaOwnerType.ProductReviews && ml.OwnerId == reviewId,
                useNoTracking: true,
                cancellationToken: ct);
            
            return mediaLinks.OrderBy(ml => ml.SortOrder).ToList();
        }

        private static void NormalizePaging(ref int page, ref int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
        }
    }
}

