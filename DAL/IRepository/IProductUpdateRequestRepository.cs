using DAL.Data.Models;

namespace DAL.IRepository;

public interface IProductUpdateRequestRepository
{
    Task<Product> GetProductByIdAsync(ulong productId, CancellationToken cancellationToken);
}