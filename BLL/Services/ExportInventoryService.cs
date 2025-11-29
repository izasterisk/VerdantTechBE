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

        var exportInventories = new List<ExportInventory>();
        Dictionary<ulong, int> productQuantities = new();
        List<ProductSerial> exportSerials = new();
        Dictionary<string, int> validateLotNumber = new(StringComparer.OrdinalIgnoreCase);
        foreach (var dto in dtos)
        {
            if(dto.MovementType == MovementType.Sale)
                throw new ArgumentException("MovementType Sale chỉ được sử dụng khi xuất hàng bán.");
            ulong? serial = null;
            if (dto.ProductSerialNumber != null)
            {
                if(dto.Quantity != 1)
                    throw new InvalidOperationException("Với sản phẩm có số sê-ri, số lượng xuất phải là 1.");
                var s = await _orderDetailRepository.GetProductSerialAsync(dto.ProductId, dto.ProductSerialNumber, dto.LotNumber, cancellationToken);
                if(s.Status is ProductSerialStatus.Sold or ProductSerialStatus.Adjustment)
                    throw new InvalidOperationException("Số sê-ri không đủ điều kiện để xuất kho.");
                if(s.Status == ProductSerialStatus.Refund && dto.MovementType != MovementType.ReturnToVendor)
                    throw new InvalidOperationException("Vì đây là hàng hoàn trả, chỉ có thể xuất kho với hình thức Trả lại nhà cung cấp.");
                serial = s.Id;
                exportSerials.Add(s);
            }
            else
            {
                if(await _orderDetailRepository.IsSerialRequiredByProductIdAsync(dto.ProductId, cancellationToken))
                    throw new InvalidOperationException("Sản phẩm này yêu cầu số sê-ri khi xuất kho.");
                if (validateLotNumber.TryGetValue(dto.LotNumber, out var count))
                {
                    validateLotNumber[dto.LotNumber] = count + dto.Quantity;
                }
                else
                {
                    validateLotNumber[dto.LotNumber] = dto.Quantity;
                }
            }
            if (productQuantities.TryGetValue(dto.ProductId, out var currentQuantity))
            {
                productQuantities[dto.ProductId] = currentQuantity + dto.Quantity;
            }
            else
            {
                productQuantities.Add(dto.ProductId, dto.Quantity);
            }
            exportInventories.Add(new ExportInventory
            {
                ProductId = dto.ProductId,
                ProductSerialId = serial,
                Quantity = dto.Quantity,
                LotNumber = dto.LotNumber,
                MovementType = dto.MovementType,
                Notes = dto.Notes,
                CreatedBy = staffId
            });
        }
        foreach (var validate in validateLotNumber)
        {
            // if (await _exportInventoryRepository.GetNumberOfProductLeftInInventoryThruLotNumberAsync(validate.Key, cancellationToken) < validate.Value)
            //     throw new InvalidOperationException($"Lô hàng với số lô {validate.Key} không còn đủ sản phẩm để xuất. Vui lòng kiểm tra lại.");
        }
        var listUlongResponseExport = await _exportInventoryRepository.CreateExportForExportWithTransactionAsync(
            exportInventories, productQuantities, exportSerials, cancellationToken);
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
}