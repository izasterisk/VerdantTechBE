using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.MediaLink
{
    public class MediaLinkItemDTO
    {
        public ulong Id { get; set; }
        public string? ImagePublicId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? Purpose { get; set; }
        public int SortOrder { get; set; }
    }
}
