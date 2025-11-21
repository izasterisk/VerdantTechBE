using AutoMapper;
using BLL.DTO.ForumPost;
using BLL.DTO.ForumComment;
using BLL.DTO.MediaLink;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BLL.Helpers;
using DAL.Data;
using System.Text.Json;

namespace BLL.Services
{
    public class ForumPostService : IForumPostService
    {
        private readonly IForumPostRepository _repo;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public ForumPostService(
            IForumPostRepository repo,
            IMapper mapper,
            INotificationService notificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<ForumPostResponseDTO>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var posts = await _repo.GetAllAsync(page, pageSize, ct);
            return _mapper.Map<List<ForumPostResponseDTO>>(posts);
        }

        public async Task<IEnumerable<ForumPostResponseDTO>> GetAllByCategoryIdAsync(
            ulong categoryId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var posts = await _repo.GetAllByCategoryIdAsync(categoryId, page, pageSize, ct);
            return _mapper.Map<List<ForumPostResponseDTO>>(posts);
        }

        public async Task<ForumPostResponseDTO?> GetDetailAsync(
            ulong id,
            CancellationToken ct = default)
        {
            var post = await _repo.GetDetailAsync(id, ct);
            if (post is null) return null;

            var dto = _mapper.Map<ForumPostResponseDTO>(post);

            dto.Images = post.MediaLinks?
                .OrderBy(m => m.SortOrder)
                .Select(m => new MediaLinkItemDTO
                {
                    Id = m.Id,
                    ImagePublicId = m.ImagePublicId,
                    ImageUrl = m.ImageUrl,
                    SortOrder = m.SortOrder,
                    Purpose = m.Purpose.ToString()
                }).ToList() ?? new List<MediaLinkItemDTO>();

            dto.Comments = post.ForumComments
                .Where(c => c.ParentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Select(MapCommentWithReplies)
                .ToList();

            return dto;
        }

        public async Task<ForumPostResponseDTO?> GetPostWithCommentsAsync(
            ulong id,
            CancellationToken ct = default)
        {
            var post = await _repo.GetPostWithCommentsAsync(id, ct);
            if (post is null) return null;

            var dto = _mapper.Map<ForumPostResponseDTO>(post);

            dto.Images = post.MediaLinks?
                .OrderBy(m => m.SortOrder)
                .Select(m => new MediaLinkItemDTO
                {
                    Id = m.Id,
                    ImagePublicId = m.ImagePublicId,
                    ImageUrl = m.ImageUrl,
                    SortOrder = m.SortOrder,
                    Purpose = m.Purpose.ToString()
                }).ToList() ?? new List<MediaLinkItemDTO>();

            dto.Comments = post.ForumComments
                .Where(c => c.ParentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Select(MapCommentWithReplies)
                .ToList();

            return dto;
        }

        public async Task<ForumPostResponseDTO> CreateAsync(
           ulong userId,
           ForumPostCreateDTO dto,
           List<MediaLinkItemDTO> addImages,
           CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            string newSlug = await GenerateUniqueSlug(dto.Title, ct);

            //var contentBlocks = dto.Content?.Select(cb => new ContentBlock
            //{
            //    Order = cb.Order,
            //    Type = cb.Type,
            //    Content = cb.Content
            //}).ToList() ?? new List<ContentBlock>();

            var contentBlocks = dto.Content?
                .Select((text, index) => new ContentBlock
                {
                    Order = index,
                    Type = "text",
                    Content = text
                })
                .ToList()
                ?? new List<ContentBlock>();



            var post = new ForumPost
            {
                ForumCategoryId = dto.ForumCategoryId,
                UserId = userId,
                Title = dto.Title,
                Slug = newSlug,
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

            List<MediaLink>? mediaEntities = null;

            if (addImages != null && addImages.Any())
            {
                mediaEntities = addImages.Select((m, index) => new MediaLink
                {
                    OwnerType = MediaOwnerType.ForumPosts,
                    ImagePublicId = m.ImagePublicId,
                    ImageUrl = m.ImageUrl,
                    Purpose = index == 0 ? MediaPurpose.Front : MediaPurpose.None,
                    SortOrder = index,
                    CreatedAt = now,
                    UpdatedAt = now
                }).ToList();
            }

            await _repo.CreateAsync(post, mediaEntities, ct);

            await _notificationService.CreateAndSendNotificationAsync(
                userId,
                " Có bài viết mới dành cho bạn!",
                $"'{post.Title}' vừa được đăng lên diễn đàn.",
                NotificationReferenceType.ForumPost,
                post.Id,
                ct
            );

            var result = _mapper.Map<ForumPostResponseDTO>(post);

            // Map images
            result.Images = mediaEntities?.Select(m => new MediaLinkItemDTO
            {
                Id = m.Id,
                ImagePublicId = m.ImagePublicId,
                ImageUrl = m.ImageUrl,
                SortOrder = m.SortOrder,
                Purpose = m.Purpose.ToString()
            }).ToList();

            return result;
        }

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

            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                post.Title = dto.Title;

                post.Slug = await GenerateUniqueSlug(dto.Title, ct);
            }

            if (dto.Tags != null)
                post.Tags = dto.Tags;

            if (dto.Content != null)
            {
                post.Content = dto.Content
                    .Select((text, index) => new ContentBlock
                    {
                        Order = index,
                        Type = "text",
                        Content = text
                    })
                    .ToList();
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

        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            await _repo.DeleteAsync(id, ct);
        }
        public async Task PinAsync(ulong id, bool isPinned, CancellationToken ct = default)
        {
            await _repo.PinAsync(id, isPinned, ct);
        }

        public async Task ChangeStatusAsync(ulong id, ForumPostStatus status, CancellationToken ct = default)
        {
            await _repo.ChangeStatusAsync(id, status, ct);
        }

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

        private async Task<string> GenerateUniqueSlug(string title, CancellationToken ct)
        {
            string baseSlug = Utils.GenerateSlug(title);
            string slug = baseSlug;
            int i = 1;

            while (await _repo.SlugExistsAsync(slug, ct))
                slug = $"{baseSlug}-{i++}";

            return slug;
        }

    }
}
