using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.Crop
{
    public class CropCreateDTO
    {
        public ulong FarmProfileId { get; set; }
        public List<CropItemCreateDto> Crops { get; set; } = new();

    }

    public class CropItemCreateDto
    {
        public string CropName { get; set; } = null!;
        public DateOnly PlantingDate { get; set; }
    }

}
