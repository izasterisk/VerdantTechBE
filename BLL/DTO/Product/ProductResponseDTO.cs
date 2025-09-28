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

        public decimal CommissionRate { get; set; } = 0.00m;

        public decimal DiscountPercentage { get; set; } = 0.00m;

        public string? EnergyEfficiencyRating { get; set; }

        public Dictionary<string, object> Specifications { get; set; } = new();

        public string? ManualUrls { get; set; }

        public string? Images { get; set; }

        public int WarrantyMonths { get; set; } = 12;

        public int StockQuantity { get; set; } = 0;

        public decimal? WeightKg { get; set; }

        public Dictionary<string, decimal> DimensionsCm { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public long ViewCount { get; set; } = 0L;

        public long SoldCount { get; set; } = 0L;

        public decimal RatingAverage { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
