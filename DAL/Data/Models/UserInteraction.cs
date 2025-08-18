using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// User likes/dislikes for various content types
/// </summary>
public partial class UserInteraction
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public TargetType TargetType { get; set; }

    public ulong TargetId { get; set; }

    public InteractionType InteractionType { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
}
