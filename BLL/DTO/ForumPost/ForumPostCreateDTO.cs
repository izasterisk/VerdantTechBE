using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace BLL.DTO.ForumPost
{
    public class ForumPostCreateDTO
    {
        public ulong ForumCategoryId { get; set; }

        public string Title { get; set; } = null!;
        //public string Slug { get; set; } = null!;
        public string? Tags { get; set; }

        // Content blocks
        //public List<ContentBlockDTO> Content { get; set; } = new();
        public List<string> Content { get; set; } = new();
        public IEnumerable<IFormFile>? AddImages { get; set; }
    }
}
