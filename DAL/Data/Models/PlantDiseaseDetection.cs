using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Plant disease detection history
/// </summary>
public partial class PlantDiseaseDetection
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public ulong? FarmProfileId { get; set; }

    [Required]
    [StringLength(500)]
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// Image size, format, etc. (JSON)
    /// </summary>
    public Dictionary<string, object> ImageMetadata { get; set; } = new();

    [StringLength(100)]
    public string? PlantType { get; set; }

    /// <summary>
    /// Array of {disease_name, confidence, severity} (JSON)
    /// </summary>
    public List<Dictionary<string, object>> DetectedDiseases { get; set; } = new();

    /// <summary>
    /// Computer vision API used
    /// </summary>
    [StringLength(50)]
    public string? AiProvider { get; set; }

    /// <summary>
    /// Raw AI API response (JSON)
    /// </summary>
    public Dictionary<string, object> AiResponse { get; set; } = new();

    /// <summary>
    /// Array of organic treatment options (JSON)
    /// </summary>
    public List<string> OrganicTreatmentSuggestions { get; set; } = new();

    /// <summary>
    /// Array of chemical treatment options (JSON)
    /// </summary>
    public List<string> ChemicalTreatmentSuggestions { get; set; } = new();

    /// <summary>
    /// Array of prevention recommendations (JSON)
    /// </summary>
    public List<string> PreventionTips { get; set; } = new();

    public UserFeedback? UserFeedback { get; set; }

    public string? FeedbackComments { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual FarmProfile? FarmProfile { get; set; }
}
