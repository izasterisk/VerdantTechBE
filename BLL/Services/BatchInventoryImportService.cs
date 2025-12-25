using BLL.DTO.BatchInventory;
using BLL.Helpers.BatchInventoryHelper;
using BLL.Helpers.Excel;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System;

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
        
        List<Dictionary<string, string>> rows;
        try
        {
            rows = ExcelHelper.ReadExcelFile(excelStream);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("worksheet") || ex.Message.Contains("Excel"))
        {
            throw new InvalidOperationException($"Lỗi khi đọc file Excel: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không mong đợi khi xử lý file Excel: {ex.Message}", ex);
        }

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
        int productIndex = 0; // Đếm số thứ tự sản phẩm
        foreach (var (rowIndex, dto) in validDtos)
        {
            productIndex++;
            var rowNumber = rowIndex + 2; // +2 vì Excel bắt đầu từ dòng 1 và có header
            var result = new BatchInventoryImportRowResultDTO
            {
                RowNumber = rowNumber,
                BatchNumber = dto.BatchNumber
            };

            try
            {
                var Product = await _db.Products.Where(p => p.Id == dto.ProductId).FirstOrDefaultAsync();
                if(Product == null)
                {
                    throw new KeyNotFoundException(
                        $"Dòng {rowNumber} - Sản phẩm số {productIndex}: " +
                        $"Sản phẩm với ID {dto.ProductId} không tồn tại trong hệ thống.");
                }
                if (!Product.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Dòng {rowNumber} - Sản phẩm số {productIndex}: " +
                        $"Sản phẩm '{Product.ProductName}' (ID: {dto.ProductId}) đang bị vô hiệu hóa . " +
                        $"Không thể nhập kho.");
                }
                var vendorId = Product.VendorId;
                // Set VendorId từ user đăng nhập (không lấy từ Excel)
                dto.VendorId = vendorId;

                // Validate product exists
                var product = await _productRepo.GetProductByIdAsync(dto.ProductId, useNoTracking: true, ct);
                if (product == null)
                {
                    throw new KeyNotFoundException(
                        $"Dòng {rowNumber} - Sản phẩm số {productIndex}: " +
                        $"Sản phẩm với ID {dto.ProductId} không tồn tại trong hệ thống.");
                }

                var productName = product.ProductName ?? $"ID {dto.ProductId}";
                var category = product.Category;
                var categoryName = category?.Name ?? "Không xác định";
                
                // Thông tin chi tiết về sản phẩm và vị trí
                var productInfo = $"Dòng {rowNumber} - Sản phẩm số {productIndex}: '{productName}' (ID: {dto.ProductId}, Danh mục: '{categoryName}')";

                // Validate vendor
                var vendorExists = await _db.Users
                    .AnyAsync(x => x.Id == vendorId && x.Role == UserRole.Vendor, ct);

                if (!vendorExists)
                {
                    throw new ArgumentException(
                        $"{productInfo}: Vendor với ID {vendorId} không tồn tại hoặc không phải là vendor.");
                }

                // Auto-generate SKU (ensure uniqueness)
                string sku;
                int attempts = 0;
                do
                {
                    sku = BatchInventoryHelper.GenerateSku(dto.ProductId);
                    attempts++;
                    if (attempts > 10)
                        throw new InvalidOperationException(
                            $"{productInfo}: Không thể tạo SKU duy nhất sau {attempts} lần thử.");
                } while (await _batchInventoryRepo.SkuExistsAsync(sku, null, ct));

                // Validate BatchNumber uniqueness
                if (await _batchInventoryRepo.BatchNumberExistsAsync(dto.BatchNumber, null, ct))
                {
                    throw new ArgumentException(
                        $"{productInfo}: BatchNumber '{dto.BatchNumber}' đã tồn tại trong hệ thống. " +
                        $"Vui lòng sử dụng BatchNumber khác.");
                }

                // Check if product requires serial numbers
                bool serialRequired = category?.SerialRequired ?? false;

                if (serialRequired)
                {
                    // Validate serial numbers
                    if (dto.SerialNumbers == null || dto.SerialNumbers.Count == 0)
                    {
                        throw new ArgumentException(
                            $"{productInfo}: Sản phẩm thuộc danh mục '{categoryName}' yêu cầu serial. " +
                            $"Vui lòng nhập danh sách số serial trong cột SerialNumbers (cần {dto.Quantity} serial).");
                    }

                    var validSerials = dto.SerialNumbers
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList();

                    if (validSerials.Count != dto.Quantity)
                    {
                        throw new ArgumentException(
                            $"{productInfo}: Số lượng serial nhập vào ({validSerials.Count}) không khớp với số lượng sản phẩm ({dto.Quantity}). " +
                            $"Cần nhập đúng {dto.Quantity} serial số.");
                    }

                    // Check for duplicates within input
                    var duplicateSerials = validSerials
                        .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    if (duplicateSerials.Any())
                    {
                        var duplicateCount = duplicateSerials.Count;
                        throw new ArgumentException(
                            $"{productInfo}: Có {duplicateCount} serial trùng lặp trong danh sách nhập: " +
                            $"{string.Join(", ", duplicateSerials.Take(10))}" +
                            (duplicateCount > 10 ? "..." : ""));
                    }

                    // Check for existing serials in database
                    var existingSerials = await _serialRepo.GetExistingSerialNumbersAsync(validSerials, ct);
                    var existingSerialsList = existingSerials.ToList();
                    if (existingSerialsList.Any())
                    {
                        throw new ArgumentException(
                            $"{productInfo}: Có {existingSerialsList.Count} serial đã tồn tại trong hệ thống: " +
                            $"{string.Join(", ", existingSerialsList.Take(10))}" +
                            (existingSerialsList.Count > 10 ? "..." : ""));
                    }
                }
                else
                {
                    // Non-serial products should not have serial numbers
                    if (dto.SerialNumbers != null && dto.SerialNumbers.Any(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        throw new ArgumentException(
                            $"{productInfo}: Sản phẩm thuộc danh mục '{categoryName}' không yêu cầu serial. " +
                            $"Vui lòng để trống cột SerialNumbers hoặc xóa dữ liệu serial trong file Excel.");
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
                result.ErrorMessage = ex.Message; // Đã có đầy đủ thông tin trong message
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
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột ProductId là bắt buộc nhưng không có giá trị. " +
                $"Vui lòng nhập ProductId (số nguyên dương) cho dòng này.");

        var productId = ExcelHelper.ParseValue<ulong>(productIdStr);
        if (!productId.HasValue)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: ProductId '{productIdStr}' không hợp lệ. " +
                $"Phải là số nguyên dương (ví dụ: 1, 123, 456).");

        dto.ProductId = productId.Value;

        // VendorId sẽ được set từ user đăng nhập, không lấy từ Excel

        // Required: BatchNumber
        if (!row.TryGetValue("BatchNumber", out var batchNumber) || string.IsNullOrWhiteSpace(batchNumber))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột BatchNumber là bắt buộc nhưng không có giá trị. " +
                $"Vui lòng nhập BatchNumber cho sản phẩm (ID: {dto.ProductId}).");

        dto.BatchNumber = batchNumber.Trim();

        if (dto.BatchNumber.Length > 100)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: BatchNumber '{dto.BatchNumber}' không được vượt quá 100 ký tự " +
                $"(hiện tại: {dto.BatchNumber.Length} ký tự).");

        if (!dto.BatchNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: BatchNumber '{dto.BatchNumber}' chứa ký tự không hợp lệ. " +
                $"Chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

        // Required: LotNumber
        if (!row.TryGetValue("LotNumber", out var lotNumber) || string.IsNullOrWhiteSpace(lotNumber))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột LotNumber là bắt buộc nhưng không có giá trị. " +
                $"Vui lòng nhập LotNumber cho sản phẩm (ID: {dto.ProductId}).");

        dto.LotNumber = lotNumber.Trim();

        if (dto.LotNumber.Length > 100)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: LotNumber '{dto.LotNumber}' không được vượt quá 100 ký tự " +
                $"(hiện tại: {dto.LotNumber.Length} ký tự).");

        if (!dto.LotNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: LotNumber '{dto.LotNumber}' chứa ký tự không hợp lệ. " +
                $"Chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

        // Required: Quantity
        if (!row.TryGetValue("Quantity", out var quantityStr) || string.IsNullOrWhiteSpace(quantityStr))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột Quantity là bắt buộc nhưng không có giá trị. " +
                $"Vui lòng nhập số lượng sản phẩm (ID: {dto.ProductId}).");

        var quantity = ExcelHelper.ParseValue<int>(quantityStr);
        if (!quantity.HasValue || quantity.Value <= 0)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Quantity '{quantityStr}' không hợp lệ cho sản phẩm (ID: {dto.ProductId}). " +
                $"Phải là số nguyên dương lớn hơn 0 (ví dụ: 1, 10, 100).");

        dto.Quantity = quantity.Value;

        // Required: UnitCostPrice
        if (!row.TryGetValue("UnitCostPrice", out var costPriceStr) || string.IsNullOrWhiteSpace(costPriceStr))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột UnitCostPrice là bắt buộc nhưng không có giá trị. " +
                $"Vui lòng nhập giá vốn đơn vị cho sản phẩm (ID: {dto.ProductId}).");

        var costPrice = ExcelHelper.ParseValue<decimal>(costPriceStr);
        if (!costPrice.HasValue || costPrice.Value < 0)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: UnitCostPrice '{costPriceStr}' không hợp lệ cho sản phẩm (ID: {dto.ProductId}). " +
                $"Phải là số không âm (>= 0), ví dụ: 0, 100000, 50000.5.");

        dto.UnitCostPrice = costPrice.Value;

        // Optional: ExpiryDate
        if (row.TryGetValue("ExpiryDate", out var expiryStr) && !string.IsNullOrWhiteSpace(expiryStr))
        {
            var expiryDate = ExcelHelper.ParseValue<DateOnly>(expiryStr);
            if (expiryDate.HasValue)
                dto.ExpiryDate = expiryDate.Value;
            else
                throw new InvalidOperationException($"Dòng {rowNumber}: ExpiryDate '{expiryStr}' không hợp lệ. Định dạng đúng: YYYY-MM-DD (ví dụ: 2025-12-31).");
        }

        // Optional: ManufacturingDate
        if (row.TryGetValue("ManufacturingDate", out var mfgStr) && !string.IsNullOrWhiteSpace(mfgStr))
        {
            var mfgDate = ExcelHelper.ParseValue<DateOnly>(mfgStr);
            if (mfgDate.HasValue)
                dto.ManufacturingDate = mfgDate.Value;
            else
                throw new InvalidOperationException($"Dòng {rowNumber}: ManufacturingDate '{mfgStr}' không hợp lệ. Định dạng đúng: YYYY-MM-DD (ví dụ: 2025-01-01).");
        }

        // Optional: SerialNumbers (comma-separated)
        if (row.TryGetValue("SerialNumbers", out var serialStr) && !string.IsNullOrWhiteSpace(serialStr))
        {
            dto.SerialNumbers = ExcelHelper.ParseCommaSeparatedList(serialStr);
        }

        return dto;
    }

    // ===========================================
    // IMPORT BY PRODUCT CODE/NAME (NEW API)
    // ===========================================

    public async Task<BatchInventoryImportResponseDTO> ImportFromExcelByCodeAsync(
        Stream excelStream,
        CancellationToken ct = default)
    {
        var response = new BatchInventoryImportResponseDTO();
        
        List<Dictionary<string, string>> rows;
        try
        {
            rows = ExcelHelper.ReadExcelFile(excelStream);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("worksheet") || ex.Message.Contains("Excel"))
        {
            throw new InvalidOperationException($"Lỗi khi đọc file Excel: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không mong đợi khi xử lý file Excel: {ex.Message}", ex);
        }

        response.TotalRows = rows.Count;

        // Validate all rows first (fail-fast approach)
        var validationErrors = new List<(int rowIndex, string error)>();
        var validRows = new List<(int rowIndex, Dictionary<string, string> row, string? productCode, string? productName)>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 2; // +2 because Excel rows start at 1 and we skip header

            try
            {
                // Parse ProductCode/ProductName
                row.TryGetValue("ProductCode", out var productCode);
                row.TryGetValue("ProductName", out var productName);
                
                productCode = string.IsNullOrWhiteSpace(productCode) ? null : productCode.Trim();
                productName = string.IsNullOrWhiteSpace(productName) ? null : productName.Trim();

                if (string.IsNullOrWhiteSpace(productCode) && string.IsNullOrWhiteSpace(productName))
                {
                    throw new InvalidOperationException(
                        $"Dòng {rowNumber}: Phải có ít nhất một trong hai: ProductCode hoặc ProductName.");
                }

                validRows.Add((i, row, productCode, productName));
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
        int productIndex = 0;
        foreach (var (rowIndex, row, productCode, productName) in validRows)
        {
            productIndex++;
            var rowNumber = rowIndex + 2;
            var result = new BatchInventoryImportRowResultDTO
            {
                RowNumber = rowNumber
            };

            try
            {
                // Find product by code or name
                var product = await FindProductByCodeOrName(productCode, productName, rowNumber, ct);
                if (product == null)
                {
                    throw new KeyNotFoundException(
                        $"Dòng {rowNumber}: Không tìm thấy sản phẩm với ProductCode '{productCode}' hoặc ProductName '{productName}'.");
                }

                // Parse DTO from row (with ProductId now known)
                var dto = ParseRowToDtoByCode(row, rowNumber, product.Id);

                var vendorId = product.VendorId;
                dto.VendorId = vendorId;

                var productNameDisplay = product.ProductName ?? $"ID {product.Id}";
                var category = product.Category;
                var categoryName = category?.Name ?? "Không xác định";
                
                var productInfo = $"Dòng {rowNumber} - Sản phẩm số {productIndex}: '{productNameDisplay}' (ID: {product.Id}, Mã: {product.ProductCode}, Danh mục: '{categoryName}')";

                result.BatchNumber = dto.BatchNumber;

                // Validate vendor
                var vendorExists = await _db.Users
                    .AnyAsync(x => x.Id == vendorId && x.Role == UserRole.Vendor, ct);

                if (!vendorExists)
                {
                    throw new ArgumentException(
                        $"{productInfo}: Vendor với ID {vendorId} không tồn tại hoặc không phải là vendor.");
                }

                // Auto-generate SKU (ensure uniqueness)
                string sku;
                int attempts = 0;
                do
                {
                    sku = BatchInventoryHelper.GenerateSku(product.Id);
                    attempts++;
                    if (attempts > 10)
                        throw new InvalidOperationException(
                            $"{productInfo}: Không thể tạo SKU duy nhất sau {attempts} lần thử.");
                } while (await _batchInventoryRepo.SkuExistsAsync(sku, null, ct));

                // Validate BatchNumber uniqueness
                if (await _batchInventoryRepo.BatchNumberExistsAsync(dto.BatchNumber, null, ct))
                {
                    throw new ArgumentException(
                        $"{productInfo}: BatchNumber '{dto.BatchNumber}' đã tồn tại trong hệ thống. " +
                        $"Vui lòng sử dụng BatchNumber khác.");
                }

                // Check if product requires serial numbers
                bool serialRequired = category?.SerialRequired ?? false;

                if (serialRequired)
                {
                    // Validate serial numbers
                    if (dto.SerialNumbers == null || dto.SerialNumbers.Count == 0)
                    {
                        throw new ArgumentException(
                            $"{productInfo}: Sản phẩm thuộc danh mục '{categoryName}' yêu cầu serial. " +
                            $"Vui lòng nhập danh sách số serial trong cột SerialNumbers (cần {dto.Quantity} serial).");
                    }

                    var validSerials = dto.SerialNumbers
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList();

                    if (validSerials.Count != dto.Quantity)
                    {
                        throw new ArgumentException(
                            $"{productInfo}: Số lượng serial nhập vào ({validSerials.Count}) không khớp với số lượng sản phẩm ({dto.Quantity}). " +
                            $"Cần nhập đúng {dto.Quantity} serial số.");
                    }

                    // Check for duplicates within input
                    var duplicateSerials = validSerials
                        .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    if (duplicateSerials.Any())
                    {
                        var duplicateCount = duplicateSerials.Count;
                        throw new ArgumentException(
                            $"{productInfo}: Có {duplicateCount} serial trùng lặp trong danh sách nhập: " +
                            $"{string.Join(", ", duplicateSerials.Take(10))}" +
                            (duplicateCount > 10 ? "..." : ""));
                    }

                    // Check for existing serials in database
                    var existingSerials = await _serialRepo.GetExistingSerialNumbersAsync(validSerials, ct);
                    var existingSerialsList = existingSerials.ToList();
                    if (existingSerialsList.Any())
                    {
                        throw new ArgumentException(
                            $"{productInfo}: Có {existingSerialsList.Count} serial đã tồn tại trong hệ thống: " +
                            $"{string.Join(", ", existingSerialsList.Take(10))}" +
                            (existingSerialsList.Count > 10 ? "..." : ""));
                    }
                }
                else
                {
                    // Non-serial products should not have serial numbers
                    if (dto.SerialNumbers != null && dto.SerialNumbers.Any(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        throw new ArgumentException(
                            $"{productInfo}: Sản phẩm thuộc danh mục '{categoryName}' không yêu cầu serial. " +
                            $"Vui lòng để trống cột SerialNumbers hoặc xóa dữ liệu serial trong file Excel.");
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

    private async Task<Product?> FindProductByCodeOrName(
        string? productCode,
        string? productName,
        int rowNumber,
        CancellationToken ct)
    {
        // Priority 1: Find by ProductCode (unique)
        if (!string.IsNullOrWhiteSpace(productCode))
        {
            var productByCode = await _db.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductCode == productCode, ct);
            
            if (productByCode != null)
                return productByCode;
        }

        // Priority 2: Find by ProductName (may have duplicates)
        if (!string.IsNullOrWhiteSpace(productName))
        {
            var productsByName = await _db.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.ProductName == productName)
                .ToListAsync(ct);

            if (productsByName.Count == 0)
            {
                return null; // Not found
            }
            else if (productsByName.Count == 1)
            {
                return productsByName.First();
            }
            else
            {
                // Multiple products with same name
                throw new InvalidOperationException(
                    $"Dòng {rowNumber}: Tìm thấy {productsByName.Count} sản phẩm với ProductName '{productName}'. " +
                    $"Vui lòng sử dụng ProductCode để xác định chính xác. " +
                    $"Các mã sản phẩm: {string.Join(", ", productsByName.Select(p => p.ProductCode).Take(5))}" +
                    (productsByName.Count > 5 ? "..." : ""));
            }
        }

        return null;
    }

    private BatchInventoryCreateDTO ParseRowToDtoByCode(
        Dictionary<string, string> row,
        int rowNumber,
        ulong productId)
    {
        var dto = new BatchInventoryCreateDTO();
        dto.ProductId = productId;

        // Required: BatchNumber
        if (!row.TryGetValue("BatchNumber", out var batchNumber) || string.IsNullOrWhiteSpace(batchNumber))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột BatchNumber là bắt buộc nhưng không có giá trị.");

        dto.BatchNumber = batchNumber.Trim();

        if (dto.BatchNumber.Length > 100)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: BatchNumber '{dto.BatchNumber}' không được vượt quá 100 ký tự " +
                $"(hiện tại: {dto.BatchNumber.Length} ký tự).");

        if (!dto.BatchNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: BatchNumber '{dto.BatchNumber}' chứa ký tự không hợp lệ. " +
                $"Chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

        // Required: LotNumber
        if (!row.TryGetValue("LotNumber", out var lotNumber) || string.IsNullOrWhiteSpace(lotNumber))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột LotNumber là bắt buộc nhưng không có giá trị.");

        dto.LotNumber = lotNumber.Trim();

        if (dto.LotNumber.Length > 100)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: LotNumber '{dto.LotNumber}' không được vượt quá 100 ký tự " +
                $"(hiện tại: {dto.LotNumber.Length} ký tự).");

        if (!dto.LotNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: LotNumber '{dto.LotNumber}' chứa ký tự không hợp lệ. " +
                $"Chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

        // Required: Quantity
        if (!row.TryGetValue("Quantity", out var quantityStr) || string.IsNullOrWhiteSpace(quantityStr))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột Quantity là bắt buộc nhưng không có giá trị.");

        var quantity = ExcelHelper.ParseValue<int>(quantityStr);
        if (!quantity.HasValue || quantity.Value <= 0)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Quantity '{quantityStr}' không hợp lệ. " +
                $"Phải là số nguyên dương lớn hơn 0 (ví dụ: 1, 10, 100).");

        dto.Quantity = quantity.Value;

        // Required: UnitCostPrice
        if (!row.TryGetValue("UnitCostPrice", out var costPriceStr) || string.IsNullOrWhiteSpace(costPriceStr))
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: Cột UnitCostPrice là bắt buộc nhưng không có giá trị.");

        var costPrice = ExcelHelper.ParseValue<decimal>(costPriceStr);
        if (!costPrice.HasValue || costPrice.Value < 0)
            throw new InvalidOperationException(
                $"Dòng {rowNumber}: UnitCostPrice '{costPriceStr}' không hợp lệ. " +
                $"Phải là số không âm (>= 0), ví dụ: 0, 100000, 50000.5.");

        dto.UnitCostPrice = costPrice.Value;

        // Optional: ExpiryDate
        if (row.TryGetValue("ExpiryDate", out var expiryStr) && !string.IsNullOrWhiteSpace(expiryStr))
        {
            var expiryDate = ExcelHelper.ParseValue<DateOnly>(expiryStr);
            if (expiryDate.HasValue)
                dto.ExpiryDate = expiryDate.Value;
            else
                throw new InvalidOperationException($"Dòng {rowNumber}: ExpiryDate '{expiryStr}' không hợp lệ. Định dạng đúng: YYYY-MM-DD (ví dụ: 2025-12-31).");
        }

        // Optional: ManufacturingDate
        if (row.TryGetValue("ManufacturingDate", out var mfgStr) && !string.IsNullOrWhiteSpace(mfgStr))
        {
            var mfgDate = ExcelHelper.ParseValue<DateOnly>(mfgStr);
            if (mfgDate.HasValue)
                dto.ManufacturingDate = mfgDate.Value;
            else
                throw new InvalidOperationException($"Dòng {rowNumber}: ManufacturingDate '{mfgStr}' không hợp lệ. Định dạng đúng: YYYY-MM-DD (ví dụ: 2025-01-01).");
        }

        // Optional: SerialNumbers (comma-separated)
        if (row.TryGetValue("SerialNumbers", out var serialStr) && !string.IsNullOrWhiteSpace(serialStr))
        {
            dto.SerialNumbers = ExcelHelper.ParseCommaSeparatedList(serialStr);
        }

        return dto;
    }
}

