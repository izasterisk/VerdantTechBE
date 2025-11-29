using DAL.Data.Models;

namespace DAL.IRepository;

public interface IPaymentRepository
{
    Task<Payment> CreatePaymentWithTransactionAsync(Payment payment, Transaction tr, CancellationToken cancellationToken = default);
    Task<Payment> UpdateFullPaymentWithTransactionAsync(Payment payment, Order order, Transaction transactions, string? customerName, CancellationToken cancellationToken = default);
    Task<Transaction> GetPaymentWithRelationByTransactionIdAsync(ulong id, CancellationToken cancellationToken = default);
}