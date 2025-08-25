using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace DAL.Data.Models;

/// <summary>
/// Master list of sustainability certifications
/// </summary>
public partial class SustainabilityCertification
{
    public ulong Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    public SustainabilityCertificationCategory Category { get; set; }

    [StringLength(255)]
    public string? IssuingBody { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<VendorSustainabilityCredential> VendorSustainabilityCredentials { get; set; } = new List<VendorSustainabilityCredential>();
}
