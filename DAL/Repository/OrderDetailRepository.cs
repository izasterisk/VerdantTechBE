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
    
    public async Task<List<ProductSerial>> GetAllProductSerialAsync(
        Dictionary<string, (string lotNumber, ulong productId)> validateSerialNumber, CancellationToken cancellationToken = default)
    {
        if (validateSerialNumber.Count == 0)
            return new List<ProductSerial>();
        var serialKeysQuery = validateSerialNumber.Keys
            .Select(k => k.ToLower())
            .ToList();
        var candidates = await _dbContext.Set<ProductSerial>()
            .AsNoTracking()
            .Include(ps => ps.BatchInventory)
            .Where(ps => serialKeysQuery.Contains(ps.SerialNumber.ToLower()))
            .ToListAsync(cancellationToken);
        
        var validResult = new List<ProductSerial>();
        var inputKeyMap = validateSerialNumber.Keys.ToDictionary(k => k.ToLower(), k => k);
        foreach (var ps in candidates)
        {
            if (!inputKeyMap.TryGetValue(ps.SerialNumber.ToLower(), out var originalKey))
            {
                continue; // Trường hợp này chỉ xảy ra nếu SQL trả về dư (rất hiếm), bỏ qua là an toàn
            }
            var (reqLotNumber, reqProductId) = validateSerialNumber[originalKey];
            // 1 Serial chỉ có 1 ProductId, nên nếu lệch -> DỮ LIỆU SAI
            if (ps.ProductId != reqProductId)
                throw new InvalidOperationException($"Dữ liệu không khớp cho Serial '{originalKey}'. ProductId yêu cầu: {reqProductId}, nhưng Database có ProductId: {ps.ProductId}.");
            if (ps.BatchInventory == null)
                throw new InvalidOperationException($"Serial '{originalKey}' tồn tại nhưng không có thông tin BatchInventory.");
            if (!ps.BatchInventory.LotNumber.Equals(reqLotNumber, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Dữ liệu không khớp cho Serial '{originalKey}'. " +
                    $"LotNumber yêu cầu: '{reqLotNumber}', nhưng Database có LotNumber: '{ps.BatchInventory.LotNumber}'.");
            }
            validResult.Add(ps);
        }
        if (validResult.Count != validateSerialNumber.Count)
        {
            var foundSerials = validResult.Select(x => x.SerialNumber)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingSerials = validateSerialNumber.Keys
                .Where(k => !foundSerials.Contains(k)).ToList();
            throw new InvalidOperationException($"Xác thực thất bại. Các serial không tìm thấy trong database: {string.Join(", ", missingSerials)}");
        }
        return validResult;
    }
    
    public async Task<bool> IsSerialRequiredByProductIdAsync(ulong productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => p.Id == productId)
            .Select(p => p.Category.SerialRequired)
            .FirstOrDefaultAsync(cancellationToken);
    }
}