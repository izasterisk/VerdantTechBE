using BLL.DTO.Address;
using DAL.Data;
using BLL.DTO.User;

namespace BLL.DTO.FarmProfile;
public class FarmProfileResponseDTO
{
    public ulong Id { get; set; }
    public UserResponseDTO User { get; set; } = null!;
    public string FarmName { get; set; } = null!;
    public decimal? FarmSizeHectares { get; set; }
    public AddressResponseDTO Address { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? PrimaryCrops { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}