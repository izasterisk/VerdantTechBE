using BLL.DTO.ForumComment;
using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IForumCommentService
    {
        Task<IEnumerable<ForumCommentResponseDTO>> GetCommentsByPostIdAsync( ulong postId, int page, int pageSize, CancellationToken ct = default);
        Task<ForumCommentResponseDTO?> GetDetailAsync( ulong id, CancellationToken ct = default);
        Task<ForumCommentResponseDTO> CreateAsync( ForumCommentCreateDTO dto,CancellationToken ct = default);
        Task<ForumCommentResponseDTO> UpdateAsync( ForumCommentUpdateDTO dto, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
        Task ChangeStatusAsync(ulong id, ForumCommentStatus status, CancellationToken ct = default);
        Task IncrementLikeAsync(ulong id, CancellationToken ct = default);
        Task IncrementDislikeAsync(ulong id, CancellationToken ct = default);
    }

}
