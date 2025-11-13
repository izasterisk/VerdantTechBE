using BLL.DTO.MediaLink;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.VendorCertificate
{
    //public class VendorCertificateCreateItemDto
    //{
    //    [Required]
    //    [StringLength(50)]
    //    public string CertificationCode { get; set; } = null!;

    //    [Required]
    //    [StringLength(255)]
    //    public string CertificationName { get; set; } = null!;
    //}

    public class VendorCertificateCreateDto
    {
        [Required]
        public ulong VendorId { get; set; }
        [Required]
        public List<string> CertificationCode { get; set; } = new();

        [Required]
        public List<string> CertificationName { get; set; } = new();

        //[Required]
        //public List<VendorCertificateCreateItemDto> Items { get; set; } = new();
    }
}
