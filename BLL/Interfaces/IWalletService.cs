using BLL.DTO;
using BLL.DTO.Wallet;

namespace BLL.Interfaces;

public interface IWalletService
{
    Task<WalletResponseDTO> ProcessWalletCreditsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<WalletCashoutRequestResponseDTO> CreateWalletCashoutRequestAsync(ulong userId, WalletCashoutRequestCreateDTO dto, CancellationToken cancellationToken = default);
    Task<WalletCashoutRequestResponseDTO> GetWalletCashoutRequestAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteWalletCashoutRequestAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<PagedResponse<WalletCashoutGetAllResponseDTO>> GetAllWalletCashoutRequestAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}