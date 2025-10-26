using DAL.Data.Models;

namespace DAL.IRepository;

public interface IPaymentRepository
{
    Task<Payment> CreatePaymentWithTransactionAsync(Payment payment, CancellationToken cancellationToken = default);
}