using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ForumComment
{
    public class ForumCommentUpdateDTO
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public string Content { get; set; } = null!;
    }

}
