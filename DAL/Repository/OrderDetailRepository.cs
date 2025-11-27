using System.Data;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class OrderDetailRepository : IOrderDetailRepository
{
    private readonly IRepository<OrderDetail> _orderDetailRepository;
    private readonly IRepository<ProductSerial> _productSerialRepository;
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<BatchInventory> _batchInventoryRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public OrderDetailRepository(IRepository<OrderDetail> orderDetailRepository,
        IRepository<ProductSerial> productSerialRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<Product> productRepository,
        IRepository<BatchInventory> batchInventoryRepository,
        VerdantTechDbContext dbContext)
    {
        _orderDetailRepository = orderDetailRepository;
        _productSerialRepository = productSerialRepository;
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _batchInventoryRepository = batchInventoryRepository;
        _dbContext = dbContext;
    }
    
    public async Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail, CancellationToken cancellationToken = default)
    {
        orderDetail.UpdatedAt = DateTime.UtcNow;
        return await _orderDetailRepository.CreateAsync(orderDetail, cancellationToken);
    }
    
    public async Task<OrderDetail> GetOrderDetailWithRelationByIdAsync(ulong orderDetailId, CancellationToken cancellationToken = default)
    {
        return await _orderDetailRepository.GetWithRelationsAsync(o => o.Id == orderDetailId,
            true, func => func.Include(od => od.Product),
            cancellationToken) ?? 
        throw new KeyNotFoundException($"Không tìm thấy chi tiết đơn hàng với ID {orderDetailId}.");
    }
    
    public async Task<ProductSerial> GetProductSerialAsync(ulong productId, string serialNumber, string lotNumber, CancellationToken cancellationToken = default)
    {
        if (await _batchInventoryRepository.AnyAsync(bi => bi.ProductId == productId && bi.LotNumber.Equals(lotNumber, StringComparison.OrdinalIgnoreCase), cancellationToken) == false)
            throw new KeyNotFoundException("Số lô không tồn tại trong hệ thống hoặc số lô không phải của sản phẩm này.");
        return await _productSerialRepository.GetAsync(p => p.ProductId == productId && 
            p.SerialNumber.Equals(serialNumber, StringComparison.OrdinalIgnoreCase), 
                   true, cancellationToken) 
            ?? throw new KeyNotFoundException("Số sê-ri không tồn tại.");
    }
    
    public async Task<bool> IsSerialRequiredByProductIdAsync(ulong productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => p.Id == productId)
            .Select(p => p.Category.SerialRequired)
            .FirstOrDefaultAsync(cancellationToken);
    }
}