using BLL.DTO.MediaLink;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ProductCertificate
{
    public class ProductCertificateWithFilesCreateDTO
    {
        [Required] public string CertificationCode { get; set; } = null!;
        [Required] public string CertificationName { get; set; } = null!;
        public List<MediaLinkItemDTO> CertificateFiles { get; set; } = new();
    }

}
