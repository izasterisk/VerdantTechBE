using BLL.DTO.MediaLink;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.VendorCertificate
{
    public class VendorCertificateCreateDTO
    {
        [Required(ErrorMessage = "id vendor không được để trống")]
        public ulong VendorId { get; set; }
        [Required(ErrorMessage = "Mã chứng chỉ không được để trống")]
        public string CertificationCode { get; set; }
        [Required(ErrorMessage = "Tên chứng chỉ không được để trống")]
        public string CertificationName { get; set; }
    }
}
