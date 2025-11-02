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
    
    public OrderDetailRepository(IRepository<OrderDetail> orderDetailRepository,
        IRepository<ProductSerial> productSerialRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<Product> productRepository,
        IRepository<BatchInventory> batchInventoryRepository)
    {
        _orderDetailRepository = orderDetailRepository;
        _productSerialRepository = productSerialRepository;
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _batchInventoryRepository = batchInventoryRepository;
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
    
    public async Task<ulong?> ValidateIdentifyNumberAsync(ulong productId, string? serialNumber, string? lotNumber, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.AnyAsync(p => p.Id == productId, cancellationToken);
        if (product == false)
            throw new KeyNotFoundException("Sản phẩm không tồn tại.");
        var rootCategoryId = await GetRootProductCategoryIdByProductIdAsync(productId, cancellationToken);
        if (rootCategoryId == 1)
        {
            if (serialNumber == null)
                throw new InvalidExpressionException("Với danh mục máy móc bắt buộc phải có số sê-ri.");
            var serial = await _productSerialRepository.GetWithRelationsAsync(
                ps => ps.ProductId == productId && ps.SerialNumber.ToUpper() == serialNumber.ToUpper(),
                true, 
                query => query.Include(u => u.BatchInventory),
                cancellationToken);
            if(serial == null || serial.Status == ProductSerialStatus.Sold)
                throw new KeyNotFoundException("Sản phẩm với số sê-ri này đã được bán hoặc số sê-ri không tồn tại trong hệ thống hoặc số sê-ri không phải của sản phẩm này.");
            if(lotNumber != null && serial.BatchInventory.LotNumber.ToUpper() != lotNumber.ToUpper())
                throw new InvalidExpressionException($"Số lô nhận vào không đúng với số lô có trong hệ thống cho sản phẩm ID {productId}, số sê-ri {serialNumber}.");
            return serial.Id;
        }
        if (rootCategoryId is 2 or 3 or 4)
        {
            if(lotNumber == null)
                throw new InvalidExpressionException("Với danh mục vật tư bắt buộc phải có số lô.");
            if (await _batchInventoryRepository.AnyAsync(bi => bi.ProductId == productId && bi.LotNumber.ToUpper() == lotNumber.ToUpper(), cancellationToken) == false)
                throw new KeyNotFoundException("Số lô không tồn tại trong hệ thống hoặc số lô không phải của sản phẩm này.");
        }
        else
        {
            throw new KeyNotFoundException("Không thể xác định danh mục sản phẩm, vui lòng liên hệ Admin.");
        }
        return null;
    }
}