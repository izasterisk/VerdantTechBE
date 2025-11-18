using BLL.DTO.MediaLink;
using DAL.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.VendorProfile
{
    public class VendorProfileCreateDTO
    {
        // Optional: thông tin User
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255, ErrorMessage = "Mật khẩu không được vượt quá 255 ký tự")]
        public string Password { get; set; } = null!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TaxCode { get; set; }
        //public UserRole Role { get; set; } = UserRole.Vendor;
        [Required]
        public string CompanyName { get; set; }
        //public string? Slug { get; set; } 
        [Required]
        public string BusinessRegistrationNumber { get; set; }
        public string? CompanyAddress { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Commune { get; set; }
        public List<string> CertificationName { get; set; } = new();
        public List<string> CertificationCode { get; set; } = new();

        


    }
}