using DAL.Data;
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
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        public ulong ProductId { get; set; }

        [Required(ErrorMessage = "Mã chứng chỉ không được để trống")]
        [StringLength(50, ErrorMessage = "Mã chứng chỉ không được vượt quá 50 ký tự")]
        public string CertificationCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên chứng chỉ không được để trống")]
        [StringLength(255, ErrorMessage = "Tên chứng chỉ không được vượt quá 255 ký tự")]
        public string CertificationName { get; set; } = null!;

        [Required(ErrorMessage = "Trạng thái chứng chỉ không được để trống")]
        public ProductCertificateStatus Status { get; set; }


        [StringLength(500, ErrorMessage = "Lý do từ chối không được vượt quá 500 ký tự")]
        public string? RejectionReason { get; set; }

    }
}
