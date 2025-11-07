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
    public class ProductCertificateCreateDTO
    {
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        public ulong ProductId { get; set; }
        [Required(ErrorMessage = "Mã chứng chỉ không được để trống")]
        public string CertificationCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên chứng chỉ không được để trống")]
        public string CertificationName { get; set; } = null!;


    }
}
