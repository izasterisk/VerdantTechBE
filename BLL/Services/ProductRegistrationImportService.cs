using BLL.DTO.ProductRegistration;
using BLL.Helpers.Excel;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using static BLL.DTO.Product.ProductUpdateDTO;
using System.Text.Json;
using System.Linq;

namespace BLL.Services;

public class ProductRegistrationImportService
{
    private readonly IProductRegistrationService _productRegistrationService;
    private readonly VerdantTechDbContext _db;

    public ProductRegistrationImportService(
        IProductRegistrationService productRegistrationService,
        VerdantTechDbContext db)
    {
        _productRegistrationService = productRegistrationService;
        _db = db;
    }

    public async Task<ProductRegistrationImportResponseDTO> ImportFromExcelAsync(
        Stream excelStream,
        ulong vendorId,
        CancellationToken ct = default)
    {
        var response = new ProductRegistrationImportResponseDTO();
        
        // Giới hạn số rows để tránh memory issue
        const int maxRows = 1000;
        const int batchSize = 20; // Commit mỗi 20 rows để tối ưu memory và performance (giảm từ 50)
        
        var rows = ExcelHelper.ReadExcelFile(excelStream);
        
        if (rows.Count > maxRows)
        {
            throw new InvalidOperationException($"File Excel có quá nhiều dòng ({rows.Count}). Vui lòng giới hạn tối đa {maxRows} dòng mỗi lần import.");
        }

        if (rows.Count == 0)
        {
            response.FailedCount = 1;
            response.Results.Add(new ProductRegistrationImportRowResultDTO
            {
                RowNumber = 0,
                IsSuccess = false,
                ErrorMessage = "File Excel không có dữ liệu hoặc không đúng định dạng."
            });
            return response;
        }

        response.TotalRows = rows.Count;

        // Validate vendor một lần trước khi xử lý
        var vendorExists = await _db.Users
            .AnyAsync(x => x.Id == vendorId && x.Role == UserRole.Vendor, ct);
        
        if (!vendorExists)
        {
            throw new InvalidOperationException($"Vendor với ID {vendorId} không tồn tại hoặc không phải là vendor.");
        }

        // Validate all rows first (fail-fast approach)
        var validationErrors = new List<(int rowIndex, string error)>();
        var validDtos = new List<(int rowIndex, ProductRegistrationCreateDTO dto)>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 2; // +2 because Excel rows start at 1 and we skip header

            try
            {
                var dto = ParseRowToDto(row, rowNumber);
                dto.VendorId = vendorId; // Set VendorId từ user đăng nhập
                
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
                response.Results.Add(new ProductRegistrationImportRowResultDTO
                {
                    RowNumber = rowIndex + 2,
                    IsSuccess = false,
                    ErrorMessage = error
                });
                response.FailedCount++;
            }
            return response;
        }

        // Cache categories để tránh query nhiều lần (tối ưu performance)
        var categoryIds = validDtos.Select(d => d.dto.CategoryId).Distinct().ToList();
        var categoryCache = await _db.ProductCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        // Validate categories từ cache
        foreach (var (rowIndex, dto) in validDtos)
        {
            if (!categoryCache.TryGetValue(dto.CategoryId, out var category))
            {
                validationErrors.Add((rowIndex, $"Category với ID {dto.CategoryId} không tồn tại."));
            }
            else if (category.ParentId == null)
            {
                validationErrors.Add((rowIndex, $"Category với ID {dto.CategoryId} là parent category, không thể sử dụng."));
            }
        }

        // Nếu có lỗi validation category, return
        if (validationErrors.Any())
        {
            foreach (var (rowIndex, error) in validationErrors)
            {
                response.Results.Add(new ProductRegistrationImportRowResultDTO
                {
                    RowNumber = rowIndex + 2,
                    IsSuccess = false,
                    ErrorMessage = error
                });
                response.FailedCount++;
            }
            return response;
        }

        // Process với batch transaction để tối ưu memory và performance
        int processedCount = 0;
        int currentBatch = 0;

        // Xử lý theo batch
        for (int batchStart = 0; batchStart < validDtos.Count; batchStart += batchSize)
        {
            var batch = validDtos.Skip(batchStart).Take(batchSize).ToList();
            
            // Tạo transaction cho mỗi batch
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var (rowIndex, dto) in batch)
                {
                    var rowNumber = rowIndex + 2;
                    var result = new ProductRegistrationImportRowResultDTO
                    {
                        RowNumber = rowNumber,
                        ProposedProductName = dto.ProposedProductName
                    };

                    try
                    {
                        // Sử dụng CreateForImportAsync thay vì CreateAsync (tối ưu - bỏ email, media hydration)
                        var created = await _productRegistrationService.CreateForImportAsync(dto, ct);

                        result.IsSuccess = true;
                        result.ProductRegistrationId = created.Id;
                        response.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = ex.Message;
                        response.FailedCount++;
                    }

                    response.Results.Add(result);
                    processedCount++;
                }

                // Commit batch
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                // Đánh dấu tất cả rows trong batch này là failed nếu chưa có result
                foreach (var (rowIndex, _) in batch)
                {
                    var existingResult = response.Results.FirstOrDefault(r => r.RowNumber == rowIndex + 2);
                    if (existingResult != null && existingResult.IsSuccess)
                    {
                        existingResult.IsSuccess = false;
                        existingResult.ErrorMessage = $"Lỗi batch: {ex.Message}";
                        response.SuccessfulCount--;
                        response.FailedCount++;
                    }
                }
                // Log error nhưng tiếp tục với batch tiếp theo
                System.Diagnostics.Debug.WriteLine($"Batch {currentBatch} failed: {ex.Message}");
            }
            
            currentBatch++;
        }

        return response;
    }

    private ProductRegistrationCreateDTO ParseRowToDto(Dictionary<string, string> row, int rowNumber)
    {
        // Parse dimensions first since it's required
        if (!row.TryGetValue("LengthCm", out var lengthStr) || string.IsNullOrWhiteSpace(lengthStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: LengthCm là bắt buộc.");

        var length = ExcelHelper.ParseValue<decimal>(lengthStr);
        if (!length.HasValue || length.Value < 0)
            throw new InvalidOperationException($"Dòng {rowNumber}: LengthCm phải là số không âm.");

        if (!row.TryGetValue("WidthCm", out var widthStr) || string.IsNullOrWhiteSpace(widthStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: WidthCm là bắt buộc.");

        var width = ExcelHelper.ParseValue<decimal>(widthStr);
        if (!width.HasValue || width.Value < 0)
            throw new InvalidOperationException($"Dòng {rowNumber}: WidthCm phải là số không âm.");

        if (!row.TryGetValue("HeightCm", out var heightStr) || string.IsNullOrWhiteSpace(heightStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: HeightCm là bắt buộc.");

        var height = ExcelHelper.ParseValue<decimal>(heightStr);
        if (!height.HasValue || height.Value < 0)
            throw new InvalidOperationException($"Dòng {rowNumber}: HeightCm phải là số không âm.");

        var dto = new ProductRegistrationCreateDTO
        {
            DimensionsCm = new DimensionsDTO
            {
                Length = length.Value,
                Width = width.Value,
                Height = height.Value
            }
        };

        // Required fields
        // VendorId sẽ được tự động lấy từ user đăng nhập, không cần nhập trong Excel

        if (!row.TryGetValue("CategoryId", out var categoryIdStr) || string.IsNullOrWhiteSpace(categoryIdStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: CategoryId là bắt buộc.");

        if (!ExcelHelper.ParseValue<ulong>(categoryIdStr).HasValue)
            throw new InvalidOperationException($"Dòng {rowNumber}: CategoryId không hợp lệ.");

        dto.CategoryId = ExcelHelper.ParseValue<ulong>(categoryIdStr)!.Value;

        if (!row.TryGetValue("ProposedProductCode", out var code) || string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException($"Dòng {rowNumber}: ProposedProductCode là bắt buộc.");

        dto.ProposedProductCode = code.Trim();

        if (dto.ProposedProductCode.Length > 100)
            throw new InvalidOperationException($"Dòng {rowNumber}: ProposedProductCode không được vượt quá 100 ký tự.");

        if (!row.TryGetValue("ProposedProductName", out var name) || string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException($"Dòng {rowNumber}: ProposedProductName là bắt buộc.");

        dto.ProposedProductName = name.Trim();

        if (dto.ProposedProductName.Length > 255)
            throw new InvalidOperationException($"Dòng {rowNumber}: ProposedProductName không được vượt quá 255 ký tự.");

        // Optional fields
        if (row.TryGetValue("Description", out var desc))
            dto.Description = desc?.Trim();

        if (string.IsNullOrWhiteSpace(dto.Description))
            dto.Description = null;

        if (dto.Description != null && dto.Description.Length > 1000)
            throw new InvalidOperationException($"Dòng {rowNumber}: Description không được vượt quá 1000 ký tự.");

        // UnitPrice
        if (!row.TryGetValue("UnitPrice", out var priceStr) || string.IsNullOrWhiteSpace(priceStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: UnitPrice là bắt buộc.");

        var price = ExcelHelper.ParseValue<decimal>(priceStr);
        if (!price.HasValue || price.Value <= 0)
            throw new InvalidOperationException($"Dòng {rowNumber}: UnitPrice phải là số dương.");

        dto.UnitPrice = price.Value;

        // EnergyEfficiencyRating (optional)
        if (row.TryGetValue("EnergyEfficiencyRating", out var ratingStr) && !string.IsNullOrWhiteSpace(ratingStr))
        {
            dto.EnergyEfficiencyRating = ratingStr.Trim();
            if (dto.EnergyEfficiencyRating.Length > 10)
                throw new InvalidOperationException($"Dòng {rowNumber}: EnergyEfficiencyRating không được vượt quá 10 ký tự.");
        }

        // WarrantyMonths (optional, default 12)
        if (row.TryGetValue("WarrantyMonths", out var warrantyStr) && !string.IsNullOrWhiteSpace(warrantyStr))
        {
            var warranty = ExcelHelper.ParseValue<int>(warrantyStr);
            if (warranty.HasValue && warranty.Value > 0)
                dto.WarrantyMonths = warranty.Value;
        }

        // WeightKg
        if (!row.TryGetValue("WeightKg", out var weightStr) || string.IsNullOrWhiteSpace(weightStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: WeightKg là bắt buộc.");

        var weight = ExcelHelper.ParseValue<decimal>(weightStr);
        if (!weight.HasValue || weight.Value < 0.001m || weight.Value > 50000m)
            throw new InvalidOperationException($"Dòng {rowNumber}: WeightKg phải từ 0.001 đến 50.000 kg.");

        dto.WeightKg = weight.Value;

        // Specifications (optional JSON)
        if (row.TryGetValue("Specifications", out var specsStr) && !string.IsNullOrWhiteSpace(specsStr))
        {
            dto.Specifications = ExcelHelper.ParseJsonToDictionary(specsStr);
        }

        // CertificationCode and CertificationName (optional, comma-separated)
        if (row.TryGetValue("CertificationCode", out var certCodeStr) && !string.IsNullOrWhiteSpace(certCodeStr))
        {
            dto.CertificationCode = ExcelHelper.ParseCommaSeparatedList(certCodeStr);
        }

        if (row.TryGetValue("CertificationName", out var certNameStr) && !string.IsNullOrWhiteSpace(certNameStr))
        {
            dto.CertificationName = ExcelHelper.ParseCommaSeparatedList(certNameStr);
        }

        // Validate certification lists match
        if (dto.CertificationCode.Count != dto.CertificationName.Count)
            throw new InvalidOperationException($"Dòng {rowNumber}: Số lượng CertificationCode và CertificationName phải bằng nhau.");

        return dto;
    }
}

