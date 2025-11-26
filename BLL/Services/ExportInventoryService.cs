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
        var staff = await _userRepository.GetVerifiedAndActiveUserByIdAsync(staffId, cancellationToken);
        if(staff.Role != UserRole.Admin && staff.Role != UserRole.Staff)
            throw new UnauthorizedAccessException("Người dùng không có quyền tạo đơn xuất kho.");
        if(dtos == null || dtos.Count == 0)
            throw new ArgumentException("Danh sách đơn xuất kho không được rỗng.");

        var exportInventories = new List<ExportInventory>();
        foreach (var dto in dtos)
        {
            if(dto.ProductSerialNumber != null && dto.Quantity != 1)
                throw new InvalidOperationException("Với sản phẩm có số sê-ri, số lượng xuất phải là 1.");
            if(dto.MovementType == MovementType.Sale)
                throw new ArgumentException("MovementType Sale chỉ được sử dụng khi xuất hàng bán.");
            var productSerial = await _orderDetailRepository.ValidateIdentifyNumberAsync(
                dto.ProductId, dto.ProductSerialNumber, dto.LotNumber, cancellationToken);
            exportInventories.Add(new ExportInventory
            {
                ProductId = dto.ProductId,
                ProductSerialId = productSerial?.Id,
                LotNumber = dto.LotNumber,
                MovementType = dto.MovementType,
                Notes = dto.Notes,
                CreatedBy = staffId
            });
        }
        var listUlongResponseExport = await _exportInventoryRepository.CreateExportNUpdateProductSerialsWithTransactionAsync(
            exportInventories, ProductSerialStatus.Adjustment, cancellationToken);
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
}