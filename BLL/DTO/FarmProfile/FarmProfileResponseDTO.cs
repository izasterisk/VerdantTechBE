using BLL.DTO.Address;
using BLL.DTO.Crops;
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
    public FarmProfileStatus Status { get; set; }
    public List<CropsResponseDTO> Crops { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}