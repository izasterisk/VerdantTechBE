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

    public string? LocationAddress { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(100)]
    public string? District { get; set; }

    [StringLength(100)]
    public string? Commune { get; set; }

    /// <summary>
    /// Main crops grown, comma-separated list
    /// </summary>
    [StringLength(500)]
    public string? PrimaryCrops { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<EnvironmentalDatum> EnvironmentalData { get; set; } = new List<EnvironmentalDatum>();
}
