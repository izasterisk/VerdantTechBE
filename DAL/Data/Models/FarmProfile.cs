using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Farm profile details for farmer users
/// </summary>
public partial class FarmProfile
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string FarmName { get; set; } = null!;

    public decimal? FarmSizeHectares { get; set; }

    public ulong? AddressId { get; set; }
    [StringLength(500)]
    public string? PrimaryCrops { get; set; }

    public FarmProfileStatus Status { get; set; } = FarmProfileStatus.Active;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Address? Address { get; set; }
    public virtual ICollection<EnvironmentalDatum> EnvironmentalData { get; set; } = new List<EnvironmentalDatum>();
}
