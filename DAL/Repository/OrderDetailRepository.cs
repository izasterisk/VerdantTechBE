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
    
    public async Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail, CancellationToken cancellationToken = default)
    {
        orderDetail.UpdatedAt = DateTime.UtcNow;
        return await _orderDetailRepository.CreateAsync(orderDetail, cancellationToken);
    }
    
    public async Task<List<OrderDetail>> GetListedOrderDetailsByIdAsync(List<ulong> orderDetailIds, CancellationToken cancellationToken = default)
    {
        List<OrderDetail> result = new List<OrderDetail>();
        foreach (var orderDetailId in orderDetailIds)
        {
            var orderDetail = await _orderDetailRepository.GetWithRelationsAsync(o => o.Id == orderDetailId,
                 true, func => func.Include(od => od.Product),
                 cancellationToken) ?? 
            throw new KeyNotFoundException($"Không tìm thấy chi tiết đơn hàng với ID {orderDetailId}.");
            
            result.Add(orderDetail);
            if (result[0].OrderId != orderDetail.OrderId)
                throw new InvalidOperationException($"Order Details nhận vào không cùng 1 đơn hàng.");
        }
        return result;
    }
    
    public async Task<OrderDetail> GetOrderDetailWithRelationByIdAsync(ulong orderDetailId, CancellationToken cancellationToken = default)
    {
        return await _orderDetailRepository.GetWithRelationsAsync(o => o.Id == orderDetailId,
            true, func => func.Include(od => od.Product),
            cancellationToken) ?? 
        throw new KeyNotFoundException($"Không tìm thấy chi tiết đơn hàng với ID {orderDetailId}.");
    }
        
    
    // public async Task<ulong> GetRootProductCategoryIdByProductIdAsync(ulong id, CancellationToken cancellationToken = default)
    // {
    //     var product = await _productRepository.GetAsync(p => p.Id == id, true, cancellationToken);
    //     if (product == null || product.IsActive == false)
    //         throw new KeyNotFoundException("Sản phẩm không còn tồn tại.");
    //     var categoryId = product.CategoryId;
    //     int check = 0;
    //     while (true)
    //     {
    //         if(check >= 10)
    //             throw new InvalidOperationException("Lỗi vòng lặp khi tìm danh mục sản phẩm gốc. Vui lòng liên hệ Admin.");
    //         check++;
    //         
    //         var category = await _productCategoryRepository.GetAsync(c => c.Id == categoryId, true, cancellationToken);
    //         if (category == null)
    //             throw new KeyNotFoundException($"Danh mục sản phẩm với ID {categoryId} không còn tồn tại.");
    //         if (category.ParentId == null)
    //             return category.Id;
    //         categoryId = category.ParentId.Value;
    //     }
    // }
    
    public async Task<ProductSerial?> ValidateIdentifyNumberAsync(ulong productId, string? serialNumber, string lotNumber, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetWithRelationsAsync(p => p.Id == productId && p.IsActive,
                          true, 
                          query => query.Include(p => p.Category), 
                          cancellationToken) ??
                      throw new KeyNotFoundException("Sản phẩm không còn tồn tại.");
        if (product.Category.SerialRequired == true)
        {
            if (serialNumber == null)
                throw new InvalidExpressionException("Với danh mục máy móc bắt buộc phải có số sê-ri.");
            var serial = await _productSerialRepository.GetWithRelationsAsync(
                ps => ps.ProductId == productId && ps.SerialNumber.Equals(serialNumber, StringComparison.OrdinalIgnoreCase),
                true, 
                query => query.Include(u => u.BatchInventory),
                cancellationToken) ?? 
                    throw new KeyNotFoundException("số sê-ri không tồn tại trong hệ thống hoặc số sê-ri không phải của sản phẩm này.");
            if(serial.Status == ProductSerialStatus.Sold || serial.Status == ProductSerialStatus.Refund)
                throw new KeyNotFoundException("Sản phẩm với số sê-ri này không đủ điều kiện để xuất.");
            if(lotNumber != null && !serial.BatchInventory.LotNumber.Equals(lotNumber, StringComparison.OrdinalIgnoreCase))
                throw new InvalidExpressionException($"Số lô nhận vào không đúng với số lô có trong hệ thống cho sản phẩm ID {productId}, số sê-ri {serialNumber}.");
            return serial;
        }
        else
        {
            if(serialNumber != null)
                throw new InvalidExpressionException("Chỉ các danh mục máy móc mới được phép nhập số sê-ri.");
            if (await _batchInventoryRepository.AnyAsync(bi => bi.ProductId == productId && bi.LotNumber.Equals(lotNumber, StringComparison.OrdinalIgnoreCase), cancellationToken) == false)
                throw new KeyNotFoundException("Số lô không tồn tại trong hệ thống hoặc số lô không phải của sản phẩm này.");
        }
        return null;
    }
}