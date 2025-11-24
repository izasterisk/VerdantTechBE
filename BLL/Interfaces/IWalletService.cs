using BLL.DTO;
using BLL.DTO.Transaction;
using BLL.DTO.Wallet;

namespace BLL.Interfaces;

public interface IWalletService
{
    Task<WalletResponseDTO> ProcessWalletCreditsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<TransactionResponseDTO> CreateWalletCashoutRequestAsync(ulong userId, WalletCashoutRequestCreateDTO dto, CancellationToken cancellationToken = default);
    Task<WalletCashoutResponseDTO> ProcessWalletCashoutRequestAsync(ulong staffId, ulong userId, WalletProcessCreateDTO dto, CancellationToken cancellationToken = default);
    Task<WalletCashoutResponseDTO> ProcessWalletCashoutRequestByPayOSAsync(ulong staffId, ulong userId, CancellationToken cancellationToken = default);
    Task<WalletCashoutRequestResponseDTO> GetWalletCashoutRequestAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteWalletCashoutRequestAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<PagedResponse<WalletCashoutRequestResponseDTO>> GetAllWalletCashoutRequestAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResponse<WalletCashoutRequestResponseDTO>> GetAllWalletCashoutRequestByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
}