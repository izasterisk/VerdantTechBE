using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.BatchInventory
{
    public class BatchInventoryCreateDto
    {
        [Required]
        public ulong ProductId { get; set; }
        [Required]
        [StringLength(100)]
        public string Sku { get; set; } = null!;
        public ulong? VendorId { get; set; }
        [Required]
        [StringLength(100)]
        public string BatchNumber { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string LotNumber { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitCostPrice { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public DateOnly? ManufacturingDate { get; set; }

    }
}
