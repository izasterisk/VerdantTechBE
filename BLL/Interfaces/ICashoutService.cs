using BLL.DTO.Cashout;

namespace BLL.Interfaces;

public interface ICashoutService
{
    Task<RefundReponseDTO> CreateCashoutRefundAsync(ulong staffId, ulong requestId, RefundCreateDTO dto, CancellationToken cancellationToken = default);
    Task<(string IPv4, string IPv6)> GetIPAddressAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default);
}