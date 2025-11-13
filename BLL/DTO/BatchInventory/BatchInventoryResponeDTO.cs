using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.BatchInventory
{
        public class BatchInventoryResponeDTO
        {
            public ulong Id { get; set; }
            public ulong ProductId { get; set; }
            public string Sku { get; set; } = null!;
            public ulong? VendorId { get; set; }
            public string BatchNumber { get; set; } = null!;
            public string LotNumber { get; set; } = null!;
            public int Quantity { get; set; }
            public decimal UnitCostPrice { get; set; }
            public DateOnly? ExpiryDate { get; set; }
            public DateOnly? ManufacturingDate { get; set; }

            public QualityCheckStatus QualityCheckStatus { get; set; }
            public ulong? QualityCheckedBy { get; set; }
            public DateTime? QualityCheckedAt { get; set; }

            public string? Notes { get; set; }

            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }

            //// Optional: lấy thêm info điều hướng
            public string? ProductName { get; set; }
            public string? VendorName { get; set; }
            public string? QualityCheckedByName { get; set; }
        }
    }

