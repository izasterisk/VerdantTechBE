using DAL.Data.Models;

namespace DAL.IRepository;

public interface IPaymentRepository
{
    Task<Payment> CreatePaymentWithTransactionAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<Payment> UpdateFullPaymentWithTransactionAsync(Payment payment, Order order, Transaction transactions,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetPaymentByGatewayPaymentIdAsync(string paymentGatewayId,
        CancellationToken cancellationToken = default);
}