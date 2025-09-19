using BLL.DTO.Address;

namespace BLL.Interfaces;

public interface IAddressService
{
    Task<AddressResponseDTO> GetAddressByIdAsync(ulong addressId, CancellationToken cancellationToken = default);
}