using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ForumComment
{
    public class ForumCommentResponseDTO
    {
        public ulong Id { get; set; }
        public ulong ForumPostId { get; set; }
        public ulong UserId { get; set; }
        public ulong? ParentId { get; set; }

        public string Content { get; set; } = null!;
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public List<ForumCommentResponseDTO> Replies { get; set; } = new();
    }
}
