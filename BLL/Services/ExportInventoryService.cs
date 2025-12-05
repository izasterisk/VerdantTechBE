using AutoMapper;
using BLL.DTO;
using BLL.DTO.ExportInventory;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class ExportInventoryService : IExportInventoryService
{
    private readonly IMapper _mapper;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IExportInventoryRepository _exportInventoryRepository;
    private readonly IUserRepository _userRepository;
    
    public ExportInventoryService(IMapper mapper, IOrderDetailRepository orderDetailRepository,
        IExportInventoryRepository exportInventoryRepository, IUserRepository userRepository)
    {
        _mapper = mapper;
        _orderDetailRepository = orderDetailRepository;
        _exportInventoryRepository = exportInventoryRepository;
        _userRepository = userRepository;
    }
    
    public async Task<List<ExportInventoryResponseDTO>> CreateExportInventoriesAsync(ulong staffId, List<ExportInventoryCreateDTO> dtos, CancellationToken cancellationToken = default)
    {
        if(dtos == null || dtos.Count == 0)
            throw new ArgumentException("Danh sách đơn xuất kho không được rỗng.");

        var exportInventories = new List<(ExportInventory Inventory, string? SerialStr)>();
        Dictionary<ulong, int> productQuantities = new();
        var returnToVendorSerials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<(ulong ProductId, string LotNumber), int> validateLotNumber = new();
        Dictionary<string, (string lotNumber, ulong productId)> validateSerialNumber = new(StringComparer.OrdinalIgnoreCase);
        foreach (var dto in dtos)
        {
            if(dto.MovementType == MovementType.Sale)
                throw new ArgumentException("MovementType Sale chỉ được sử dụng khi xuất hàng bán.");
            var check = await _orderDetailRepository.IsSerialRequiredByProductIdAsync(dto.ProductId, cancellationToken);
            string? serial = null;
            if (check)
            {
                if (dto.SerialNumber == null)
                    throw new InvalidOperationException($"Sản phẩm có ID {dto.ProductId} yêu cầu số sê-ri khi xuất kho.");
                if(dto.Quantity != 1)
                    throw new InvalidOperationException("Với sản phẩm có số sê-ri, số lượng xuất phải là 1.");
                if (validateSerialNumber.TryGetValue(dto.SerialNumber, out var lotNumberQuantity))
                    throw new ArgumentException($"Số sê-ri {dto.SerialNumber} bị trùng. Vui lòng kiểm tra lại.");
                validateSerialNumber.Add(dto.SerialNumber, (dto.LotNumber, dto.ProductId));
                serial = dto.SerialNumber;
                if (dto.MovementType == MovementType.ReturnToVendor)
                    returnToVendorSerials.Add(dto.SerialNumber);
            }
            else
            {
                if (dto.SerialNumber != null)
                    throw new InvalidOperationException($"Sản phẩm có ID {dto.ProductId} không yêu cầu số sê-ri khi xuất kho.");
                var compositeKey = (dto.ProductId, dto.LotNumber.Trim().ToUpper());
                if (validateLotNumber.TryGetValue(compositeKey, out var count))
                    throw new InvalidOperationException($"Số lô {dto.LotNumber} cho sản phẩm ID {dto.ProductId} bị lặp lại, vui lòng gộp số lượng.");
                validateLotNumber[compositeKey] = dto.Quantity;
            }
            if (productQuantities.TryGetValue(dto.ProductId, out var currentQuantity))
            {
                productQuantities[dto.ProductId] = currentQuantity + dto.Quantity;
            }
            else
            {
                productQuantities.Add(dto.ProductId, dto.Quantity);
            }
            exportInventories.Add((new ExportInventory
            {
                ProductId = dto.ProductId,
                // ProductSerialId = serial,
                Quantity = dto.Quantity,
                LotNumber = dto.LotNumber,
                MovementType = dto.MovementType,
                Notes = dto.Notes,
                CreatedBy = staffId
            }, serial));
        }
        var validatedSerials = await _orderDetailRepository.GetAllProductSerialToExportAsync(validateSerialNumber, cancellationToken);
        if (validateSerialNumber.Count > 0)
        {
            foreach (var s in validatedSerials)
            {
                if(s.Status is ProductSerialStatus.Sold or ProductSerialStatus.Adjustment)
                    throw new InvalidOperationException("Số sê-ri không đủ điều kiện để xuất kho.");
                if(s.Status == ProductSerialStatus.Refund && !returnToVendorSerials.Contains(s.SerialNumber))
                    throw new InvalidOperationException($"Đối với số sê-ri {s.SerialNumber}. Vì đây là hàng hoàn trả, chỉ có thể xuất kho với hình thức 'Trả lại nhà cung cấp'.");
            }
        }
        foreach (var validate in validateLotNumber)
        {
            if (await _exportInventoryRepository.GetNumberOfProductLeftInInventoryThruLotNumberAsync(validate.Key.LotNumber, validate.Key.ProductId, cancellationToken) < validate.Value)
                throw new InvalidOperationException($"Lô hàng với số lô {validate.Key} không còn đủ sản phẩm để xuất. Vui lòng kiểm tra lại.");
        }
        var serialLookup = validatedSerials.ToDictionary(s => s.SerialNumber.ToUpper(), s => s);
        var finalExportList = new List<ExportInventory>();
        foreach (var exportInventory in exportInventories)
        {
            var inventory = exportInventory.Inventory;
            if (exportInventory.SerialStr != null )
            {
                if (serialLookup.TryGetValue(exportInventory.SerialStr.ToUpper(), out var serialEntity))
                {
                    inventory.ProductSerialId = serialEntity.Id;
                }
                else
                {
                    throw new InvalidOperationException($"Lỗi hệ thống: Không mapping được ID cho serial {exportInventory.SerialStr}.");
                }
            }
            finalExportList.Add(inventory);
        }
        var listUlongResponseExport = await _exportInventoryRepository.CreateExportForExportWithTransactionAsync(
            finalExportList, productQuantities, validatedSerials, cancellationToken);
        var createdExportInventories = await _exportInventoryRepository.GetListedExportInventoriesByIdsAsync(
            listUlongResponseExport, cancellationToken);
        return _mapper.Map<List<ExportInventoryResponseDTO>>(createdExportInventories);
    }
    
    public async Task<ExportInventoryResponseDTO> GetExportInventoryByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var exportInventory = await _exportInventoryRepository.GetExportInventoryByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn xuất kho với ID {id}.");
        return _mapper.Map<ExportInventoryResponseDTO>(exportInventory);
    }
    
    public async Task<PagedResponse<ExportInventoryResponseDTO>> GetAllExportInventoriesAsync(int page, int pageSize, string? movementType = null, CancellationToken cancellationToken = default)
    {
        var (exportInventories, totalCount) = await _exportInventoryRepository.GetAllExportInventoriesAsync(page, pageSize, movementType, cancellationToken);
        var exportInventoryDtos = _mapper.Map<List<ExportInventoryResponseDTO>>(exportInventories);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResponse<ExportInventoryResponseDTO>
        {
            Data = exportInventoryDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
    
    public async Task<IdentityNumberDTO> GetIdentityNumbersAsync(ulong productId, CancellationToken cancellationToken = default)
    {
        var check = await _orderDetailRepository.IsSerialRequiredByProductIdAsync(productId, cancellationToken);
        var response = new IdentityNumberDTO();
        if (!check)
        {
            var lotNumbers = await _exportInventoryRepository.GetAllLotNumbersByProductIdAsync(productId, cancellationToken);
            var lotQuantities = await _exportInventoryRepository.GetNumberOfProductsLeftInInventoryThruLotNumbersAsync(lotNumbers, cancellationToken);
            response.LotNumberInfo = lotQuantities.Select(lq => new LotNumberResponseDTO
            {
                LotNumber = lq.LotNumber,
                Quantity = lq.RemainingQuantity
            }).ToList();
            response.SerialNumberInfo = null;
        }
        else
        {
            var serialNumbers = await _exportInventoryRepository.GetSerialNumbersWithLotNumbersByProductIdAsync(productId, cancellationToken);
            response.SerialNumberInfo = serialNumbers.Select(sn => new SerialNumberResponseDTO
            {
                LotNumber = sn.LotNumber,
                SerialNumber = sn.SerialNumber
            }).ToList();
            response.LotNumberInfo = null;
        }
        return response;
    }
    
    public async Task<IdentityNumberDTO> GetAllIdentityNumbersExportedByOrderDetailIdAsync(ulong orderDetailId, CancellationToken cancellationToken = default)
    {
        var exports = await _exportInventoryRepository.GetAllExportInventoryByOrderDetailIdAsync(orderDetailId, cancellationToken);
        if(exports.Count == 0)
            throw new KeyNotFoundException($"Không tìm thấy đơn xuất kho nào liên quan đến OrderDetailId {orderDetailId}.");
        var response = new IdentityNumberDTO();
        if (exports[0].ProductSerialId == null)
        {
            response.LotNumberInfo = exports.Select(export => new LotNumberResponseDTO
            {
                LotNumber = export.LotNumber,
                Quantity = export.Quantity
            }).ToList();
            response.SerialNumberInfo = null;
        }
        else
        {
            response.SerialNumberInfo = new List<SerialNumberResponseDTO>();
            foreach (var export in exports)
            {
                string serial = null!;
                if (export.ProductSerialId != null)
                {
                    var serialNumber = await _exportInventoryRepository.GetSerialNumberByIdAsync(export.ProductSerialId.Value, cancellationToken);
                    serial = serialNumber;
                }
                response.SerialNumberInfo.Add(new SerialNumberResponseDTO
                {
                    LotNumber = export.LotNumber,
                    SerialNumber = serial
                });
            }
            response.LotNumberInfo = null;
        }
        return response;
    }
}