using DAL.Data.Models;

namespace DAL.IRepository;

public interface ITransactionRepository
{
    Task<Transaction> GetTransactionForPaymentByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken cancellationToken = default);
}