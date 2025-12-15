using BLL.DTO.BatchInventory;
using BLL.Helpers.BatchInventoryHelper;
using BLL.Helpers.Excel;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class BatchInventoryImportService
{
    private readonly IBatchInventoryService _batchInventoryService;
    private readonly IBatchInventoryRepository _batchInventoryRepo;
    private readonly IProductRepository _productRepo;
    private readonly IProductSerialRepository _serialRepo;
    private readonly VerdantTechDbContext _db;

    public BatchInventoryImportService(
        IBatchInventoryService batchInventoryService,
        IBatchInventoryRepository batchInventoryRepo,
        IProductRepository productRepo,
        IProductSerialRepository serialRepo,
        VerdantTechDbContext db)
    {
        _batchInventoryService = batchInventoryService;
        _batchInventoryRepo = batchInventoryRepo;
        _productRepo = productRepo;
        _serialRepo = serialRepo;
        _db = db;
    }

    public async Task<BatchInventoryImportResponseDTO> ImportFromExcelAsync(
        Stream excelStream,
        CancellationToken ct = default)
    {
        var response = new BatchInventoryImportResponseDTO();
        var rows = ExcelHelper.ReadExcelFile(excelStream);

        response.TotalRows = rows.Count;

        // Validate all rows first (fail-fast approach)
        var validationErrors = new List<(int rowIndex, string error)>();
        var validDtos = new List<(int rowIndex, BatchInventoryCreateDTO dto)>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 2; // +2 because Excel rows start at 1 and we skip header

            try
            {
                var dto = ParseRowToDto(row, rowNumber);
                validDtos.Add((i, dto));
            }
            catch (Exception ex)
            {
                validationErrors.Add((i, ex.Message));
            }
        }

        // If there are validation errors, return them without processing
        if (validationErrors.Any())
        {
            foreach (var (rowIndex, error) in validationErrors)
            {
                response.Results.Add(new BatchInventoryImportRowResultDTO
                {
                    RowNumber = rowIndex + 2,
                    IsSuccess = false,
                    ErrorMessage = error
                });
                response.FailedCount++;
            }
            return response;
        }

        // Process each valid row
        foreach (var (rowIndex, dto) in validDtos)
        {
            var rowNumber = rowIndex + 2;
            var result = new BatchInventoryImportRowResultDTO
            {
                RowNumber = rowNumber,
                BatchNumber = dto.BatchNumber
            };

            try
            {
                // Validate product exists
                var product = await _productRepo.GetProductByIdAsync(dto.ProductId, useNoTracking: true, ct);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Sản phẩm với ID {dto.ProductId} không tồn tại.");
                }

                // Validate vendor if provided
                if (dto.VendorId.HasValue)
                {
                    var vendorExists = await _db.Users
                        .AnyAsync(x => x.Id == dto.VendorId.Value && x.Role == UserRole.Vendor, ct);
                    
                    if (!vendorExists)
                    {
                        throw new ArgumentException($"Vendor với ID {dto.VendorId.Value} không tồn tại hoặc không phải là vendor.");
                    }
                }

                // Auto-generate SKU (ensure uniqueness)
                string sku;
                int attempts = 0;
                do
                {
                    sku = BatchInventoryHelper.GenerateSku(dto.ProductId);
                    attempts++;
                    if (attempts > 10)
                        throw new InvalidOperationException("Không thể tạo SKU duy nhất sau nhiều lần thử.");
                } while (await _batchInventoryRepo.SkuExistsAsync(sku, null, ct));

                // Validate BatchNumber uniqueness
                if (await _batchInventoryRepo.BatchNumberExistsAsync(dto.BatchNumber, null, ct))
                {
                    throw new ArgumentException($"BatchNumber '{dto.BatchNumber}' đã tồn tại trong hệ thống.");
                }

                // Check if product requires serial numbers
                var category = product.Category;
                bool serialRequired = category?.SerialRequired ?? false;

                if (serialRequired)
                {
                    // Validate serial numbers
                    if (dto.SerialNumbers == null || dto.SerialNumbers.Count == 0)
                    {
                        throw new ArgumentException("Sản phẩm yêu cầu serial: Vui lòng nhập danh sách số serial.");
                    }

                    var validSerials = dto.SerialNumbers
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList();

                    if (validSerials.Count != dto.Quantity)
                    {
                        throw new ArgumentException($"Số lượng serial ({validSerials.Count}) phải bằng số lượng sản phẩm ({dto.Quantity}).");
                    }

                    // Check for duplicates within input
                    var duplicateSerials = validSerials
                        .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    if (duplicateSerials.Any())
                    {
                        throw new ArgumentException($"Có serial trùng lặp trong danh sách nhập: {string.Join(", ", duplicateSerials)}");
                    }

                    // Check for existing serials in database
                    var existingSerials = await _serialRepo.GetExistingSerialNumbersAsync(validSerials, ct);
                    if (existingSerials.Any())
                    {
                        throw new ArgumentException($"Các serial sau đã tồn tại trong hệ thống: {string.Join(", ", existingSerials)}");
                    }
                }
                else
                {
                    // Non-serial products should not have serial numbers
                    if (dto.SerialNumbers != null && dto.SerialNumbers.Any(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        throw new ArgumentException("Sản phẩm này không yêu cầu serial. Vui lòng không nhập số serial.");
                    }
                }

                // Create BatchInventory
                var created = await _batchInventoryService.CreateAsync(dto, ct);

                result.IsSuccess = true;
                result.BatchInventoryId = created.Id;
                result.Sku = created.Sku;
                response.SuccessfulCount++;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                response.FailedCount++;
            }

            response.Results.Add(result);
        }

        return response;
    }

    private BatchInventoryCreateDTO ParseRowToDto(Dictionary<string, string> row, int rowNumber)
    {
        var dto = new BatchInventoryCreateDTO();

        // Required: ProductId
        if (!row.TryGetValue("ProductId", out var productIdStr) || string.IsNullOrWhiteSpace(productIdStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: ProductId là bắt buộc.");

        var productId = ExcelHelper.ParseValue<ulong>(productIdStr);
        if (!productId.HasValue)
            throw new InvalidOperationException($"Dòng {rowNumber}: ProductId không hợp lệ.");

        dto.ProductId = productId.Value;

        // Optional: VendorId
        if (row.TryGetValue("VendorId", out var vendorIdStr) && !string.IsNullOrWhiteSpace(vendorIdStr))
        {
            var vendorId = ExcelHelper.ParseValue<ulong>(vendorIdStr);
            if (vendorId.HasValue)
                dto.VendorId = vendorId.Value;
        }

        // Required: BatchNumber
        if (!row.TryGetValue("BatchNumber", out var batchNumber) || string.IsNullOrWhiteSpace(batchNumber))
            throw new InvalidOperationException($"Dòng {rowNumber}: BatchNumber là bắt buộc.");

        dto.BatchNumber = batchNumber.Trim();

        if (dto.BatchNumber.Length > 100)
            throw new InvalidOperationException($"Dòng {rowNumber}: BatchNumber không được vượt quá 100 ký tự.");

        if (!dto.BatchNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
            throw new InvalidOperationException($"Dòng {rowNumber}: BatchNumber chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

        // Required: LotNumber
        if (!row.TryGetValue("LotNumber", out var lotNumber) || string.IsNullOrWhiteSpace(lotNumber))
            throw new InvalidOperationException($"Dòng {rowNumber}: LotNumber là bắt buộc.");

        dto.LotNumber = lotNumber.Trim();

        if (dto.LotNumber.Length > 100)
            throw new InvalidOperationException($"Dòng {rowNumber}: LotNumber không được vượt quá 100 ký tự.");

        if (!dto.LotNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
            throw new InvalidOperationException($"Dòng {rowNumber}: LotNumber chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

        // Required: Quantity
        if (!row.TryGetValue("Quantity", out var quantityStr) || string.IsNullOrWhiteSpace(quantityStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: Quantity là bắt buộc.");

        var quantity = ExcelHelper.ParseValue<int>(quantityStr);
        if (!quantity.HasValue || quantity.Value <= 0)
            throw new InvalidOperationException($"Dòng {rowNumber}: Quantity phải là số nguyên dương.");

        dto.Quantity = quantity.Value;

        // Required: UnitCostPrice
        if (!row.TryGetValue("UnitCostPrice", out var costPriceStr) || string.IsNullOrWhiteSpace(costPriceStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: UnitCostPrice là bắt buộc.");

        var costPrice = ExcelHelper.ParseValue<decimal>(costPriceStr);
        if (!costPrice.HasValue || costPrice.Value < 0)
            throw new InvalidOperationException($"Dòng {rowNumber}: UnitCostPrice phải là số không âm.");

        dto.UnitCostPrice = costPrice.Value;

        // Optional: ExpiryDate
        if (row.TryGetValue("ExpiryDate", out var expiryStr) && !string.IsNullOrWhiteSpace(expiryStr))
        {
            var expiryDate = ExcelHelper.ParseValue<DateOnly>(expiryStr);
            if (expiryDate.HasValue)
                dto.ExpiryDate = expiryDate.Value;
        }

        // Optional: ManufacturingDate
        if (row.TryGetValue("ManufacturingDate", out var mfgStr) && !string.IsNullOrWhiteSpace(mfgStr))
        {
            var mfgDate = ExcelHelper.ParseValue<DateOnly>(mfgStr);
            if (mfgDate.HasValue)
                dto.ManufacturingDate = mfgDate.Value;
        }

        // Optional: SerialNumbers (comma-separated)
        if (row.TryGetValue("SerialNumbers", out var serialStr) && !string.IsNullOrWhiteSpace(serialStr))
        {
            dto.SerialNumbers = ExcelHelper.ParseCommaSeparatedList(serialStr);
        }

        return dto;
    }
}

