using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.VendorProfiles;

public class VendorProfilesDTO
{
    public ulong Id { get; set; }

    [Required(ErrorMessage = "UserId is required")]
    public ulong UserId { get; set; }

    [Required(ErrorMessage = "Company name is required")]
    [StringLength(255, ErrorMessage = "Company name cannot exceed 255 characters")]
    public string CompanyName { get; set; } = null!;

    [Required(ErrorMessage = "Slug is required")]
    [StringLength(255, ErrorMessage = "Slug cannot exceed 255 characters")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must be lowercase alphanumeric with hyphens only")]
    public string Slug { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Business registration number cannot exceed 100 characters")]
    public string? BusinessRegistrationNumber { get; set; }

    [StringLength(50, ErrorMessage = "Tax code cannot exceed 50 characters")]
    public string? TaxCode { get; set; }

    public string? CompanyAddress { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    public Dictionary<string, object> BankAccountInfo { get; set; } = new();

    [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100")]
    public decimal CommissionRate { get; set; } = 10.00m;

    [Range(0, 5, ErrorMessage = "Rating average must be between 0 and 5")]
    public decimal RatingAverage { get; set; } = 0.00m;

    [Range(0, int.MaxValue, ErrorMessage = "Total reviews must be non-negative")]
    public int TotalReviews { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// List of sustainability credentials with verification status
    /// </summary>
    public List<VendorSustainabilityCredentialDTO> SustainabilityCredentials { get; set; } = new();
}

public class VendorSustainabilityCredentialDTO
{
    public ulong Id { get; set; }
    public ulong CertificationId { get; set; }
    public string CertificationCode { get; set; } = null!;
    public string CertificationName { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string CertificateUrl { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? RejectionReason { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedByName { get; set; }
}