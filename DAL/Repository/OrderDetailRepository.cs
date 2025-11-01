using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class OrderDetailRepository : IOrderDetailRepository
{
    private readonly IRepository<OrderDetail> _orderDetailRepository;
    private readonly IRepository<ProductSerial> _productSerialRepository;
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<Product> _productRepository;
    
    public OrderDetailRepository(
        IRepository<OrderDetail> orderDetailRepository,
        IRepository<ProductSerial> productSerialRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<Product> productRepository)
    {
        _orderDetailRepository = orderDetailRepository;
        _productSerialRepository = productSerialRepository;
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
    }
    
    public async Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail)
    {
        orderDetail.CreatedAt = DateTime.UtcNow;
        return await _orderDetailRepository.CreateAsync(orderDetail);
    }
    
    public async Task<ulong> GetRootProductCategoryIdByProductIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetAsync(p => p.Id == id, true, cancellationToken);
        if (product == null || product.IsActive == false)
            throw new KeyNotFoundException("Sản phẩm không còn tồn tại.");
        var categoryId = product.CategoryId;
        int check = 0;
        while (true)
        {
            if(check >= 10)
                throw new InvalidOperationException("Lỗi vòng lặp khi tìm danh mục sản phẩm gốc. Vui lòng liên hệ Admin.");
            check++;
            
            var category = await _productCategoryRepository.GetAsync(c => c.Id == categoryId, true, cancellationToken);
            if (category == null)
                throw new KeyNotFoundException($"Danh mục sản phẩm với ID {categoryId} không còn tồn tại.");
            if (category.ParentId == null)
                return category.Id;
            categoryId = category.ParentId.Value;
        }
    }
}