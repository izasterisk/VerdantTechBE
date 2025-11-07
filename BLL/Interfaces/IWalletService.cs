using BLL.DTO.Wallet;

namespace BLL.Interfaces;

public interface IWalletService
{
    Task<WalletResponseDTO> ProcessWalletCreditsAsync(ulong vendorId, CancellationToken cancellationToken = default);
}