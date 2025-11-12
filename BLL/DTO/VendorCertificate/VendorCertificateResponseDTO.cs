using DAL.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.VendorCertificate
{
    public class VendorCertificateResponseDTO
    {
        public ulong Id { get; set; }
        public ulong VendorId { get; set; }
        public string CertificationCode { get; set; }
        public string CertificationName { get; set; }
        public VendorCertificateStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public ulong? VerifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
