using DAL.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ProductCertificate
{
    public class ProductCertificateResponseDTO
    {
        public ulong Id { get; set; }

        public ulong ProductId { get; set; }

        public string? CertificationCode { get; set; } 

        public string? CertificationName { get; set; } 

        public ProductCertificateStatus Status { get; set; } 
   
        public string? RejectionReason { get; set; }

        public DateTime UploadedAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public ulong? VerifiedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
