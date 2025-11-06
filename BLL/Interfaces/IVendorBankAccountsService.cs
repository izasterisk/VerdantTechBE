using BLL.DTO.VendorBankAccount;

namespace BLL.Interfaces;

public interface IVendorBankAccountsService
{
    Task<VendorBankAccountResponseDTO> CreateVendorBankAccountAsync(ulong userId, VendorBankAccountCreateDTO dto, CancellationToken cancellationToken = default);
    Task<VendorBankAccountResponseDTO> UpdateVendorBankAccountAsync(ulong accountId, VendorBankAccountUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<List<VendorBankAccountResponseDTO>> GetAllVendorBankAccountsByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
}