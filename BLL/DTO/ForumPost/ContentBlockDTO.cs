using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ForumPost
{
    public class ContentBlockDTO
    {
        public int Order { get; set; }
        public string Type { get; set; } = null!; 
        public string Content { get; set; } = null!;
    }

}
