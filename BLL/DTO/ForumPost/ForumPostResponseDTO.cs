using BLL.DTO.ForumComment;
using BLL.DTO.MediaLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ForumPost
{
    public class ForumPostResponseDTO
    {
        public ulong Id { get; set; }
        public ulong ForumCategoryId { get; set; }
        public ulong UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Tags { get; set; }
        public bool IsPinned { get; set; }
        public string Status { get; set; } = "";

        public long ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Content blocks
        public List<ContentBlockDTO> Content { get; set; } = new();

        // Image list
        public List<MediaLinkItemDTO> Images { get; set; } = new();

        // Comments 
        public List<ForumCommentResponseDTO> Comments { get; set; } = new();
    }

}
