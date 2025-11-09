namespace BLL.Interfaces;

public interface ICashoutService
{
    Task<(string IPv4, string IPv6)> GetIPAddressAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default);
}