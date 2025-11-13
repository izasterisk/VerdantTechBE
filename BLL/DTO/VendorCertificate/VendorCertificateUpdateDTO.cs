using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.VendorCertificate
{
    public class VendorCertificateUpdateDTO
    {
        public ulong Id { get; set; }
        [Required(ErrorMessage = "id vendor không được để trống")]
        public ulong VendorId { get; set; }
        [Required(ErrorMessage = "Mã chứng chỉ không được để trống")]
        public List<string> CertificationCode { get; set; } = new();
        [Required(ErrorMessage = "Tên chứng chỉ không được để trống")]
        public List<string> CertificationName { get; set; } = new();
    }
}
