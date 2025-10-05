using DAL.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ProductRegistration
{
    public class ProductRegistrationReponseDTO
    {
        public ulong Id { get; set; }

        public ulong VendorId { get; set; }

        public ulong CategoryId { get; set; }

        public string ProposedProductCode { get; set; } = null!;

        public string ProposedProductName { get; set; } = null!;

        public string? Description { get; set; }

        public decimal UnitPrice { get; set; }

        public string? EnergyEfficiencyRating { get; set; }

        public Dictionary<string, object>? Specifications { get; set; } 

        public string? ManualUrls { get; set; }

        public string? Images { get; set; }

        public int WarrantyMonths { get; set; } = 12;

        public decimal? WeightKg { get; set; }

        public Dictionary<string, object>? DimensionsCm { get; set; } 

        public ProductRegistrationStatus Status { get; set; } = ProductRegistrationStatus.Pending;

        public string? RejectionReason { get; set; }

        public ulong? ApprovedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }
    }
}
