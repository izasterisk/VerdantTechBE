using AutoMapper;
using BLL.DTO.ForumPost;
using BLL.DTO.ForumComment;
using BLL.DTO.MediaLink;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ForumPostService : IForumPostService
    {
        private readonly IForumPostRepository _repo;
        //private readonly ICloudinaryService _cloudinary;
        private readonly IMapper _mapper;

        public ForumPostService(
            IForumPostRepository repo,
            //ICloudinaryService cloudinary,
            IMapper mapper)
        {
            _repo = repo;
            //_cloudinary = cloudinary;
            _mapper = mapper;
        }

        // ===================== GET ALL =====================
        public async Task<IEnumerable<ForumPostResponseDTO>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var posts = await _repo.GetAllAsync(page, pageSize, ct);
            var result = _mapper.Map<List<ForumPostResponseDTO>>(posts);
            return result;
        }

        // ===================== GET ALL BY CATEGORY =====================
        public async Task<IEnumerable<ForumPostResponseDTO>> GetAllByCategoryIdAsync(
            ulong categoryId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var posts = await _repo.GetAllByCategoryIdAsync(categoryId, page, pageSize, ct);
            var result = _mapper.Map<List<ForumPostResponseDTO>>(posts);
            return result;
        }

        // ===================== GET DETAIL =====================
        public async Task<ForumPostResponseDTO?> GetDetailAsync(
            ulong id,
            CancellationToken ct = default)
        {
            var post = await _repo.GetDetailAsync(id, ct);
            if (post is null) return null;

            var dto = _mapper.Map<ForumPostResponseDTO>(post);

            // chỉ lấy comment cha, replies build trong MapCommentWithReplies
            dto.Comments = post.ForumComments
                .Where(c => c.ParentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Select(MapCommentWithReplies)
                .ToList();

            return dto;
        }

        // ===================== GET POST WITH COMMENTS (DEEP) =====================
        public async Task<ForumPostResponseDTO?> GetPostWithCommentsAsync(
            ulong id,
            CancellationToken ct = default)
        {
            var post = await _repo.GetPostWithCommentsAsync(id, ct);
            if (post is null) return null;

            var dto = _mapper.Map<ForumPostResponseDTO>(post);

            dto.Comments = post.ForumComments
                .Where(c => c.ParentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Select(MapCommentWithReplies)
                .ToList();

            return dto;
        }

        // ===================== CREATE =====================
        public async Task<ForumPostResponseDTO> CreateAsync(
           ulong userId,
           ForumPostCreateDTO dto,
           List<MediaLinkItemDTO> addImages,
           CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var contentBlocks = dto.Content?.Select(cb => new ContentBlock
            {
                Order = cb.Order,
                Type = cb.Type,
                Content = cb.Content
            }).ToList() ?? new List<ContentBlock>();

            var post = new ForumPost
            {
                ForumCategoryId = dto.ForumCategoryId,
                UserId = userId,
                Title = dto.Title,
                Slug = dto.Slug,
                Tags = dto.Tags,
                Content = contentBlocks,
                ViewCount = 0,
                LikeCount = 0,
                DislikeCount = 0,
                IsPinned = false,
                Status = ForumPostStatus.Visible,
                CreatedAt = now,
                UpdatedAt = now
            };

            // Convert MediaLinkItemDTO -> MediaLink
            List<MediaLink>? mediaEntities = null;

            if (addImages != null && addImages.Count > 0)
            {
                mediaEntities = addImages.Select((m, index) => new MediaLink
                {
                    OwnerType = MediaOwnerType.ForumPosts,
                    OwnerId = post.Id, // lưu ý: lúc này Id có thể vẫn là 0, repo có thể cần chỉnh nếu muốn chính xác
                    ImagePublicId = m.ImagePublicId,
                    ImageUrl = m.ImageUrl,
                    Purpose = index == 0 ? MediaPurpose.Front : MediaPurpose.None,
                    SortOrder = m.SortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                }).ToList();
            }

            await _repo.CreateAsync(post, mediaEntities, ct);

            var result = _mapper.Map<ForumPostResponseDTO>(post);
            return result;
        }

        // ===================== UPDATE =====================
        public async Task<ForumPostResponseDTO> UpdateAsync(
            ulong id,
            ForumPostUpdateDTO dto,
            List<MediaLinkItemDTO> addImages,
            List<string> removedImages,
            CancellationToken ct = default)
        {
            var post = await _repo.GetDetailAsync(id, ct);
            if (post is null)
                throw new KeyNotFoundException($"ForumPost {id} không tồn tại.");

            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Title)) post.Title = dto.Title;
            if (!string.IsNullOrWhiteSpace(dto.Slug)) post.Slug = dto.Slug;
            if (dto.Tags != null) post.Tags = dto.Tags;

            if (dto.Content != null)
            {
                post.Content = dto.Content.Select(cb => new ContentBlock
                {
                    Order = cb.Order,
                    Type = cb.Type,
                    Content = cb.Content
                }).ToList();
            }

            post.UpdatedAt = now;

            List<MediaLink>? addMedia = null;
            if (addImages != null && addImages.Any())
            {
                addMedia = addImages.Select(m => new MediaLink
                {
                    OwnerType = MediaOwnerType.ForumPosts,
                    OwnerId = id,
                    ImagePublicId = m.ImagePublicId,
                    ImageUrl = m.ImageUrl,
                    Purpose = m.SortOrder == 0 ? MediaPurpose.Front : MediaPurpose.None,
                    SortOrder = m.SortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                }).ToList();
            }

            await _repo.UpdateAsync(post, addMedia, removedImages, ct);

            return _mapper.Map<ForumPostResponseDTO>(post);
        }

        // ===================== DELETE =====================
        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            await _repo.DeleteAsync(id, ct);
        }

        // ===================== PIN / STATUS =====================
        public async Task PinAsync(ulong id, bool isPinned, CancellationToken ct = default)
        {
            await _repo.PinAsync(id, isPinned, ct);
        }

        public async Task ChangeStatusAsync(ulong id, ForumPostStatus status, CancellationToken ct = default)
        {
            await _repo.ChangeStatusAsync(id, status, ct);
        }

        // ===================== COUNTERS =====================
        public async Task IncrementViewAsync(ulong id, CancellationToken ct = default)
        {
            await _repo.IncrementViewAsync(id, ct);
        }

        public async Task IncrementLikeAsync(ulong id, CancellationToken ct = default)
        {
            await _repo.IncrementLikeAsync(id, ct);
        }

        public async Task IncrementDislikeAsync(ulong id, CancellationToken ct = default)
        {
            await _repo.IncrementDislikeAsync(id, ct);
        }

        private ForumCommentResponseDTO MapCommentWithReplies(ForumComment c)
        {
            var dto = _mapper.Map<ForumCommentResponseDTO>(c);

            dto.Replies = c.InverseParent
                .OrderByDescending(x => x.CreatedAt)
                .Select(MapCommentWithReplies)
                .ToList();

            return dto;
        }
    }
}
