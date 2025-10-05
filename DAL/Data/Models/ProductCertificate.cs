using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Product sustainability credentials uploaded for verification
/// </summary>
public partial class ProductCertificate
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    [Required]
    [StringLength(50)]
    public string CertificationCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string CertificationName { get; set; } = null!;

    /// <summary>
    /// URL to uploaded certificate image/file
    /// </summary>
    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    [StringLength(500)]
    public string? PublicUrl { get; set; }

    public ProductCertificateStatus Status { get; set; } = ProductCertificateStatus.Pending;

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
    public virtual User? VerifiedByNavigation { get; set; }
}
