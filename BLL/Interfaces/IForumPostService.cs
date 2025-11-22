using BLL.DTO.ForumPost;
using BLL.DTO.MediaLink;
using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IForumPostService
    {
        Task<IEnumerable<ForumPostResponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<ForumPostResponseDTO>> GetAllByCategoryIdAsync(ulong categoryId, int page, int pageSize, CancellationToken ct = default);
        Task<ForumPostResponseDTO?> GetDetailAsync(ulong id, CancellationToken ct = default);
        Task<ForumPostResponseDTO?> GetPostWithCommentsAsync(ulong id, CancellationToken ct = default);
        Task<ForumPostResponseDTO> CreateAsync( ulong userId, ForumPostCreateDTO dto, List<MediaLinkItemDTO> addImages, CancellationToken ct = default);
        Task<ForumPostResponseDTO> UpdateAsync( ulong id, ForumPostUpdateDTO dto, List<MediaLinkItemDTO> addImages, List<string> removedImages, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
        Task PinAsync(ulong id, bool isPinned, CancellationToken ct = default);
        Task ChangeStatusAsync(ulong id, ForumPostStatus status, CancellationToken ct = default);
        Task IncrementViewAsync(ulong id, CancellationToken ct = default);
        Task IncrementLikeAsync(ulong id, CancellationToken ct = default);
        Task IncrementDislikeAsync(ulong id, CancellationToken ct = default);
    }

}