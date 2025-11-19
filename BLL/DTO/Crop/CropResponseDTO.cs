using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.Crop
{
    public class CropResponseDTO
    {
        public ulong Id { get; set; }
        public ulong FarmProfileId { get; set; }
        public string CropName { get; set; } = null!;
        public DateOnly PlantingDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
