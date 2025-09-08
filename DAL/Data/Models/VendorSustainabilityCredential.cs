using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace DAL.Data.Models;

/// <summary>
/// Vendor uploaded sustainability certificates for verification
/// </summary>
public partial class VendorSustainabilityCredential
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public ulong CertificationId { get; set; }

    [Required]
    [StringLength(500)]
    public string CertificateUrl { get; set; } = null!;

    [Required]
    public VendorSustainabilityCredentialStatus Status { get; set; } = VendorSustainabilityCredentialStatus.Pending;

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual VendorProfile Vendor { get; set; } = null!;
    public virtual SustainabilityCertification Certification { get; set; } = null!;
    public virtual User? VerifiedByNavigation { get; set; }
}
