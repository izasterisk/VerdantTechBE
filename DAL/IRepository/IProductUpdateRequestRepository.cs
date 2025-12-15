using DAL.Data.Models;

namespace DAL.IRepository;

public interface IProductUpdateRequestRepository
{
    Task<Product> GetProductByIdAsync(ulong productId, CancellationToken cancellationToken);
    Task<bool> IsThereAnyPendingRequestAsync(ulong productId, CancellationToken cancellationToken);
}