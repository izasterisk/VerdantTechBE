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
    public class VendorProfileResponseDTO
    {
        public ulong Id { get; set; }

        // Optional: thông tin User
        public ulong UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TaxCode { get; set; }
        public string? AvatarUrl { get; set; }
        public UserStatus Status { get; set; }

        public string CompanyName { get; set; } = null!;
        public string? Slug { get; set; } = null!;
        public string? BusinessRegistrationNumber { get; set; }
        public string? CompanyAddress { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Commune { get; set; }
   
        public List<MediaLinkItemDTO> Files { get; set; } = new();

        public DateTime? VerifiedAt { get; set; }
        public ulong? VerifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}