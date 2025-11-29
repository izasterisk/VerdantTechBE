using BLL.DTO.Cashout;
using BLL.DTO.Transaction;

namespace BLL.Interfaces;

public interface ICashoutService
{
    Task<TransactionResponseDTO> CreateCashoutRefundAsync(ulong staffId, ulong requestId, RefundCreateDTO dtos, CancellationToken cancellationToken = default);
    Task<(string IPv4, string IPv6)> GetIPAddressAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default);
}