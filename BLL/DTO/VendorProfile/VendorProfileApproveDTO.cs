using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.VendorProfile
{
    public class VendorProfileApproveDTO
    {
        public ulong Id { get; set; }
        public ulong VerifiedBy { get; set; }
    }
}
