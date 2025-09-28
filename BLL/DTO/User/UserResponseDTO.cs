using BLL.DTO.Address;

namespace BLL.DTO.User;

public class UserResponseDTO
{
    public ulong Id { get; set; }

    public string Email { get; set; } = null!;

    // public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = "customer";

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool IsVerified { get; set; } = false;

    // public string? VerificationToken { get; set; }

    // public DateTime? VerificationSentAt { get; set; }

    public string? AvatarUrl { get; set; }

    public string Status { get; set; } = "active";

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Danh sách địa chỉ của người dùng
    public List<AddressResponseDTO> Addresses { get; set; } = new List<AddressResponseDTO>();
}