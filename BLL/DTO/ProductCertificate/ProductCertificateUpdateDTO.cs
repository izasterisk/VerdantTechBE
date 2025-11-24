using DAL.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ProductCertificate
{
    public class ProductCertificateUpdateDTO
    {
        [Required(ErrorMessage = "Id chứng chỉ là bắt buộc")]
        public ulong Id { get; set; }

        public ulong? ProductId { get; set; }

        public string? CertificationCode { get; set; }

        public string? CertificationName { get; set; }

        // Các trường khác (nếu mở lại comment) cũng làm tương tự:
        // public ProductCertificateStatus? Status { get; set; }
        // public string? RejectionReason { get; set; }
    }
}
