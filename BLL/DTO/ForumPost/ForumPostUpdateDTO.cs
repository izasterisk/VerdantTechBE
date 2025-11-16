using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ForumPost
{
    public class ForumPostUpdateDTO
    {
        public ulong Id { get; set; }
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Tags { get; set; }
        public List<ContentBlockDTO>? Content { get; set; }

        // Ảnh thêm mới
        public IEnumerable<IFormFile>? AddImages { get; set; }

        // Ảnh cần xóa (publicId)
        public IEnumerable<string>? RemoveImagePublicIds { get; set; }
    }

}
