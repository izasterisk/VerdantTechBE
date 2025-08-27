using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.SupportedBanks;

public class SupportedBanksCreateDTO
{
    // public ulong Id { get; set; }

    [Required(ErrorMessage = "Bank code is required")]
    [StringLength(20, ErrorMessage = "Bank code cannot exceed 20 characters")]
    public string BankCode { get; set; } = null!;

    [Required(ErrorMessage = "Bank name is required")]
    [StringLength(255, ErrorMessage = "Bank name cannot exceed 255 characters")]
    public string BankName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Please provide a valid URL")]
    public string? ImageUrl { get; set; }

    // public bool IsActive { get; set; } = true;

    // public DateTime CreatedAt { get; set; }

    // public DateTime UpdatedAt { get; set; }
}