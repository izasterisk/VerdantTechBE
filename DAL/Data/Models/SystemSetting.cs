using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// System configuration settings
/// </summary>
public partial class SystemSetting
{
    public ulong Id { get; set; }

    [Required]
    [StringLength(100)]
    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public SettingType SettingType { get; set; } = SettingType.String;

    public string? Description { get; set; }

    /// <summary>
    /// Can be exposed to frontend
    /// </summary>
    public bool IsPublic { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
