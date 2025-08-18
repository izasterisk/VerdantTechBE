using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// User likes/dislikes for various content types
/// </summary>
public partial class UserInteraction
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string TargetType { get; set; } = null!;

    public ulong TargetId { get; set; }

    public string InteractionType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
