using System.ComponentModel.DataAnnotations;
using DAL.Data;
using DAL.Data.Models;

namespace BLL.DTO.ProductRegistration
{
    public class ProductRegistrationChangeStatusDTO
    {
        [Required] public ulong Id { get; set; }
        [Required] public ProductRegistrationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public ulong? ApprovedBy { get; set; }
    }
}
