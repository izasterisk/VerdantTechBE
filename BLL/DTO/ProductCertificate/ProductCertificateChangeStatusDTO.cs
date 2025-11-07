using DAL.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ProductCertificate
{
    public class ProductCertificateChangeStatusDTO
    {
        public ulong Id { get; set; }
        [Required]
        public ProductCertificateStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public ulong? VerifiedBy { get; set; }
    }
}
