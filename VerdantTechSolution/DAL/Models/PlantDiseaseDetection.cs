using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Plant disease detection history
/// </summary>
public partial class PlantDiseaseDetection
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public ulong? FarmProfileId { get; set; }

    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// Image size, format, etc.
    /// </summary>
    public string? ImageMetadata { get; set; }

    public string? PlantType { get; set; }

    /// <summary>
    /// Array of {disease_name, confidence, severity}
    /// </summary>
    public string? DetectedDiseases { get; set; }

    /// <summary>
    /// Computer vision API used
    /// </summary>
    public string? AiProvider { get; set; }

    /// <summary>
    /// Raw AI API response
    /// </summary>
    public string? AiResponse { get; set; }

    /// <summary>
    /// Array of organic treatment options
    /// </summary>
    public string? OrganicTreatmentSuggestions { get; set; }

    /// <summary>
    /// Array of chemical treatment options
    /// </summary>
    public string? ChemicalTreatmentSuggestions { get; set; }

    /// <summary>
    /// Array of prevention recommendations
    /// </summary>
    public string? PreventionTips { get; set; }

    public string? UserFeedback { get; set; }

    public string? FeedbackComments { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual FarmProfile? FarmProfile { get; set; }

    public virtual User User { get; set; } = null!;
}
