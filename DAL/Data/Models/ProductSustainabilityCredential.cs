using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Product sustainability credentials uploaded for verification
/// </summary>
public partial class ProductSustainabilityCredential
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public ulong CertificationId { get; set; }

    /// <summary>
    /// URL to uploaded certificate image/file
    /// </summary>
    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    public ProductSustainabilityCredentialStatus Status { get; set; } = ProductSustainabilityCredentialStatus.Pending;

    /// <summary>
    /// Rejection reason if status is rejected
    /// </summary>
    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual SustainabilityCertification Certification { get; set; } = null!;
    public virtual User? VerifiedByUser { get; set; }
}
