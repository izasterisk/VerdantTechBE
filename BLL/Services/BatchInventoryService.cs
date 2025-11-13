using AutoMapper;
using BLL.DTO.BatchInventory;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services
{
    public class BatchInventoryService : IBatchInventoryService
    {
        private readonly IBatchInventoryRepository _repo;
        private readonly IProductRepository _productRepo;
        private readonly IVendorProfileRepository _vendorRepo;
        private readonly IMapper _mapper;

        public BatchInventoryService(
            IBatchInventoryRepository repo,
            IProductRepository productRepo,
            IVendorProfileRepository vendorRepo,
            IMapper mapper)
        {
            _repo = repo;
            _productRepo = productRepo;
            _vendorRepo = vendorRepo;
            _mapper = mapper;
        }

        // GET ALL
        public async Task<IEnumerable<BatchInventoryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(page, pageSize, ct);
            return _mapper.Map<IEnumerable<BatchInventoryDto>>(items);
        }

        // GET BY PRODUCT
        public async Task<IEnumerable<BatchInventoryDto>> GetByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default)
        {
            // CHECK PRODUCT EXISTS
            var product = await _productRepo.GetProductByIdAsync(productId, useNoTracking: true, ct);
            if (product == null)
                throw new KeyNotFoundException($"Product ID {productId} not found");

            var items = await _repo.GetByProductIdAsync(productId, page, pageSize, ct);
            return _mapper.Map<IEnumerable<BatchInventoryDto>>(items);
        }

        // GET BY VENDOR
        public async Task<IEnumerable<BatchInventoryDto>> GetByVendorIdAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default)
        {
            var vendor = await _vendorRepo.GetByIdAsync(vendorId, ct);
            if (vendor == null)
                throw new KeyNotFoundException($"Vendor ID {vendorId} not found");

            var items = await _repo.GetByVendorIdAsync(vendorId, page, pageSize, ct);
            return _mapper.Map<IEnumerable<BatchInventoryDto>>(items);
        }

        // GET BY ID
        public async Task<BatchInventoryDto?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);

            if (entity == null)
                throw new KeyNotFoundException($"BatchInventory ID {id} not found");

            return _mapper.Map<BatchInventoryDto>(entity);
        }

        // CREATE
        public async Task<BatchInventoryDto> CreateAsync(BatchInventoryCreateDto dto, CancellationToken ct = default)
        {
            // CHECK PRODUCT
            var product = await _productRepo.GetProductByIdAsync(dto.ProductId, useNoTracking: true, ct);
            if (product == null)
                throw new KeyNotFoundException($"Product ID {dto.ProductId} not found");

            // CHECK VENDOR
            if (dto.VendorId.HasValue)
            {
                var vendor = await _vendorRepo.GetByIdAsync(dto.VendorId.Value, ct);
                if (vendor == null)
                    throw new KeyNotFoundException($"Vendor ID {dto.VendorId.Value} not found");
            }

            var entity = _mapper.Map<BatchInventory>(dto);
            var created = await _repo.CreateAsync(entity, ct);

            return _mapper.Map<BatchInventoryDto>(created);
        }

        // UPDATE
        public async Task<BatchInventoryDto> UpdateAsync(BatchInventoryUpdateDto dto, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(dto.Id, ct);
            if (existing == null)
                throw new KeyNotFoundException($"BatchInventory ID {dto.Id} not found");

            // Check product
            var product = await _productRepo.GetProductByIdAsync(dto.ProductId, useNoTracking: true, ct);
            if (product == null)
                throw new KeyNotFoundException($"Product ID {dto.ProductId} not found");

            // Check vendor
            if (dto.VendorId.HasValue)
            {
                var vendor = await _vendorRepo.GetByIdAsync(dto.VendorId.Value, ct);
                if (vendor == null)
                    throw new KeyNotFoundException($"Vendor ID {dto.VendorId.Value} not found");
            }

            _mapper.Map(dto, existing);

            await _repo.UpdateAsync(existing, ct);

            return _mapper.Map<BatchInventoryDto>(existing);
        }

        // DELETE
        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var exists = await _repo.GetByIdAsync(id, ct);
            if (exists == null)
                throw new KeyNotFoundException($"BatchInventory ID {id} not found");

            await _repo.DeleteAsync(id, ct);
        }

        // QUALITY CHECK
        public async Task QualityCheckAsync(ulong id, BatchInventoryQualityCheckDto dto, CancellationToken ct = default)
        {
            var exists = await _repo.GetByIdAsync(id, ct);

            if (exists == null)
                throw new KeyNotFoundException($"BatchInventory ID {id} not found");

            await _repo.QualityCheckAsync(
                id: id,
                status: dto.Status,
                qualityCheckedByUserId: dto.QualityCheckedByUserId,
                notes: dto.Notes,
                ct: ct
            );
        }
    }
}
