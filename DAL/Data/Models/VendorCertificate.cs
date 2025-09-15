using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace DAL.Data.Models;

/// <summary>
/// Vendor uploaded sustainability certificates for verification
/// </summary>
public partial class VendorCertificate
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    [Required]
    [StringLength(50)]
    public string CertificationCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string CertificationName { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string CertificateUrl { get; set; } = null!;

    [Required]
    public VendorCertificateStatus Status { get; set; } = VendorCertificateStatus.Pending;

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual VendorProfile Vendor { get; set; } = null!;
    public virtual User? VerifiedByNavigation { get; set; }
}
