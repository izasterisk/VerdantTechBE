using DAL.Data.Models;

namespace DAL.IRepository;

public interface ITransactionRepository
{
    Task<Transaction> GetTransactionForPaymentByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken cancellationToken = default);
    Task<Transaction> GetPaymentTransactionByOrderIdAsync(ulong orderId, CancellationToken cancellationToken = default);
    Task<Transaction> GetTransactionForSubscriptionByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken cancellationToken = default);
}