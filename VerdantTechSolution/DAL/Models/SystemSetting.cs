using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// System configuration settings
/// </summary>
public partial class SystemSetting
{
    public ulong Id { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string? SettingType { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Can be exposed to frontend
    /// </summary>
    public bool? IsPublic { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
