using BLL.DTO.ProductRegistration;
using BLL.Helpers.Excel;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using static BLL.DTO.Product.ProductUpdateDTO;
using System.Text.Json;

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
        CancellationToken ct = default)
    {
        var response = new ProductRegistrationImportResponseDTO();
        var rows = ExcelHelper.ReadExcelFile(excelStream);

        response.TotalRows = rows.Count;

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

        // Process each valid row
        foreach (var (rowIndex, dto) in validDtos)
        {
            var rowNumber = rowIndex + 2;
            var result = new ProductRegistrationImportRowResultDTO
            {
                RowNumber = rowNumber,
                ProposedProductName = dto.ProposedProductName
            };

            try
            {
                // Validate vendor exists
                if (!await _db.Users.AnyAsync(x => x.Id == dto.VendorId && x.Role == UserRole.Vendor, ct))
                {
                    throw new InvalidOperationException($"Vendor với ID {dto.VendorId} không tồn tại hoặc không phải là vendor.");
                }

                // Validate category exists and is not parent
                var category = await _db.ProductCategories
                    .FirstOrDefaultAsync(x => x.Id == dto.CategoryId, ct);
                
                if (category == null)
                {
                    throw new InvalidOperationException($"Category với ID {dto.CategoryId} không tồn tại.");
                }

                if (category.ParentId == null)
                {
                    throw new InvalidOperationException($"Category với ID {dto.CategoryId} là parent category, không thể sử dụng.");
                }

                // Create ProductRegistration (without images and certificates - can be added later)
                var created = await _productRegistrationService.CreateAsync(
                    dto,
                    manualUrl: null,
                    manualPublicUrl: null,
                    addImages: new List<DTO.MediaLink.MediaLinkItemDTO>(),
                    addCertificates: new List<DTO.MediaLink.MediaLinkItemDTO>(),
                    ct);

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
        if (!row.TryGetValue("VendorId", out var vendorIdStr) || string.IsNullOrWhiteSpace(vendorIdStr))
            throw new InvalidOperationException($"Dòng {rowNumber}: VendorId là bắt buộc.");

        if (!ExcelHelper.ParseValue<ulong>(vendorIdStr).HasValue)
            throw new InvalidOperationException($"Dòng {rowNumber}: VendorId không hợp lệ.");

        dto.VendorId = ExcelHelper.ParseValue<ulong>(vendorIdStr)!.Value;

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

