using AutoMapper;
using BLL.DTO.Address;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly IMapper _mapper;
    
    public AddressService(IAddressRepository addressRepository, IMapper mapper)
    {
        _addressRepository = addressRepository;
        _mapper = mapper;
    }
    
    public async Task<AddressResponseDTO> GetAddressByIdAsync(ulong addressId, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetAddressByIdAsync(addressId, cancellationToken);
        if (address == null)
            throw new KeyNotFoundException("Không tìm thấy địa chỉ");
        
        return _mapper.Map<AddressResponseDTO>(address);
    }
}