using System;
using System.Collections.Generic;
using BLL.DTO.MediaLink;

namespace BLL.DTO.Product
{
    public class ProductListItemDTO
    {
        public ulong Id { get; set; }
        public ulong VendorId { get; set; }
        public ulong CategoryId { get; set; }

        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }

        public decimal UnitPrice { get; set; }
        public int? EnergyEfficiencyRating { get; set; }
        public decimal CommissionRate { get; set; }


        public string? ManualUrls { get; set; }
        public string? PublicUrl { get; set; }

        public int WarrantyMonths { get; set; }
        public decimal? WeightKg { get; set; }

        // Ảnh (MediaLink)
        public List<MediaLinkItemDTO> Images { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
