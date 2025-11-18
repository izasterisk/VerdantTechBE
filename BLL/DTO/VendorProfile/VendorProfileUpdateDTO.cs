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
    public class VendorProfileUpdateDTO
    {
        public ulong Id { get; set; }
        [Required]
        public string CompanyName { get; set; }
        //public string? Slug { get; set; }
        [Required]
        public string BusinessRegistrationNumber { get; set; }
        public string? CompanyAddress { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Commune { get; set; }

        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TaxCode { get; set; }
     
    }
}