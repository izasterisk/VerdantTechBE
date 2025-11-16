using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.VendorProfile
{
    public class VendorProfileRejectDTO
    {
        public ulong Id { get; set; }
        public ulong VerifiedBy { get; set; }
        [Required]
        public string RejectionReason { get; set; } = null!;
    }
}
