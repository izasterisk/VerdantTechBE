using BLL.DTO.MediaLink;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.Product
{
    public class ProductResponseDTO
    {
        public ulong Id { get; set; }
        
        public ulong CategoryId { get; set; }

        public ulong VendorId { get; set; }

        public string ProductCode { get; set; } = null!;

        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal CommissionRate { get; set; }

        public decimal DiscountPercentage { get; set; } 

        public string? EnergyEfficiencyRating { get; set; }

        public Dictionary<string, object> Specifications { get; set; } = new();

        //public string? ManualUrls { get; set; }

        //public string? Images { get; set; }
        public string? ManualUrls { get; set; }
        public string? PublicUrl { get; set; }

        public List<MediaLinkItemDTO> Images { get; set; } = new();

        public int WarrantyMonths { get; set; } = 12;

        public int StockQuantity { get; set; } = 0;

        [Required]
        public decimal WeightKg { get; set; }

        public Dictionary<string, decimal> DimensionsCm { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public long ViewCount { get; set; } 

        public long SoldCount { get; set; }

        public decimal RatingAverage { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
