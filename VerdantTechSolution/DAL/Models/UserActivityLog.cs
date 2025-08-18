using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// User activity tracking for analytics
/// </summary>
public partial class UserActivityLog
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    /// <summary>
    /// login, view_product, search, etc.
    /// </summary>
    public string ActivityType { get; set; } = null!;

    public string? ActivityDetails { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? SessionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
