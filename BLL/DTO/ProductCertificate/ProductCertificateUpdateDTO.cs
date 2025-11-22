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
        public ulong Id { get; set; }

        public ulong ProductId { get; set; }

        public string CertificationCode { get; set; } = null!;

        public string CertificationName { get; set; } = null!;

        //[Required(ErrorMessage = "Trạng thái chứng chỉ không được để trống")]
        //public ProductCertificateStatus Status { get; set; }

        //public string? RejectionReason { get; set; }

    }
}
