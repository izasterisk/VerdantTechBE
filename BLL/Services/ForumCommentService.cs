using AutoMapper;
using BLL.DTO.ForumComment;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.Data;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ForumCommentService : IForumCommentService
    {
        private readonly IForumCommentRepository _repo;
        private readonly IMapper _mapper;
        private readonly INotificationService _notification;

        public ForumCommentService(
            IForumCommentRepository repo,
            IMapper mapper,
            INotificationService notification)
        {
            _repo = repo;
            _mapper = mapper;
            _notification = notification;
        }

        public async Task<IEnumerable<ForumCommentResponseDTO>> GetCommentsByPostIdAsync(
            ulong postId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var comments = await _repo.GetCommentsByPostIdAsync(postId, page, pageSize, ct);
            return comments.Select(MapWithReplies);
        }

        public async Task<ForumCommentResponseDTO?> GetDetailAsync(
            ulong id,
            CancellationToken ct = default)
        {
            var c = await _repo.GetDetailAsync(id, ct);
            return c == null ? null : MapWithReplies(c);
        }

        public async Task<ForumCommentResponseDTO> CreateAsync(ForumCommentCreateDTO dto,CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var entity = new ForumComment
            {
                ForumPostId = dto.ForumPostId,
                UserId = dto.UserId,
                ParentId = dto.ParentId,
                Content = dto.Content,
                CreatedAt = now,
                UpdatedAt = now,
                Status = ForumCommentStatus.Visible
            };

            await _repo.CreateAsync(entity, ct);


            if (dto.ParentId == null)
            {
                var postOwnerId = await _repo.GetPostOwnerIdAsync(dto.ForumPostId, ct);

                if (postOwnerId != dto.UserId)
                {
                    await _notification.CreateAndSendNotificationAsync(
                        userId: postOwnerId,
                        title: "Có bình luận mới trong bài viết của bạn",
                        message: dto.Content,
                        referenceType: NotificationReferenceType.ForumPost,
                        referenceId: dto.ForumPostId,
                        cancellationToken: ct
                    );
                }
            }
            else
            {
                var parent = await _repo.GetDetailAsync(dto.ParentId.Value, ct);

                if (parent != null && parent.UserId != dto.UserId)
                {
                    await _notification.CreateAndSendNotificationAsync(
                        userId: parent.UserId,
                        title: "Ai đó đã phản hồi bình luận của bạn",
                        message: dto.Content,
                        referenceType: NotificationReferenceType.ForumComment,
                        referenceId: parent.Id,
                        cancellationToken: ct
                    );
                }
            }

            return MapWithReplies(entity);
        }

        public async Task<ForumCommentResponseDTO> UpdateAsync(
            ForumCommentUpdateDTO dto,
            CancellationToken ct = default)
        {
            var c = await _repo.GetDetailAsync(dto.Id, ct);
            if (c == null)
                throw new KeyNotFoundException("Comment không tồn tại");

            c.Content = dto.Content;
            c.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(c, ct);

            return MapWithReplies(c);
        }

        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var c = await _repo.GetDetailAsync(id, ct);
            if (c == null) return;

            // CASCADE DELETE
            var replies = await _repo.GetAllRepliesRecursiveAsync(id, ct);

            foreach (var reply in replies)
                await _repo.DeleteAsync(reply.Id, ct);

            await _repo.DeleteAsync(id, ct);
        }

        public async Task ChangeStatusAsync(
            ulong id,
            ForumCommentStatus status,
            CancellationToken ct = default)
        {
            await _repo.ChangeStatusAsync(id, status, ct);
        }

        public async Task IncrementLikeAsync(ulong id, CancellationToken ct = default)
        {
            await _repo.IncrementLikeAsync(id, ct);
        }

        public async Task IncrementDislikeAsync(ulong id, CancellationToken ct = default)
        {
            await _repo.IncrementDislikeAsync(id, ct);
        }

        private ForumCommentResponseDTO MapWithReplies(ForumComment c)
        {
            var dto = _mapper.Map<ForumCommentResponseDTO>(c);

            dto.Replies = c.InverseParent
                .Select(MapWithReplies)
                .ToList();

            return dto;
        }
    }
}
