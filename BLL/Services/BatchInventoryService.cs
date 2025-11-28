using AutoMapper;
using BLL.DTO.BatchInventory;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class BatchInventoryService : IBatchInventoryService
    {
        private readonly IBatchInventoryRepository _repo;
        private readonly IProductRepository _productRepo;
        private readonly IProductSerialRepository _serialRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public BatchInventoryService(
            IBatchInventoryRepository repo,
            IProductRepository productRepo,
            IProductSerialRepository serialRepo,
            IUserRepository userRepo,
            IMapper mapper)
        {
            _repo = repo;
            _productRepo = productRepo;
            _serialRepo = serialRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BatchInventoryResponeDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(page, pageSize, ct);
            return _mapper.Map<IEnumerable<BatchInventoryResponeDTO>>(items);
        }

        public async Task<IEnumerable<BatchInventoryResponeDTO>> GetByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default)
        {
            var product = await _productRepo.GetProductByIdAsync(productId, useNoTracking: true, ct);
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm có ID {productId}");

            var items = await _repo.GetByProductIdAsync(productId, page, pageSize, ct);
            return _mapper.Map<IEnumerable<BatchInventoryResponeDTO>>(items);
        }

        public async Task<IEnumerable<BatchInventoryResponeDTO>> GetByVendorIdAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default)
        {
            var items = await _repo.GetByVendorIdAsync(vendorId, page, pageSize, ct);
            return _mapper.Map<IEnumerable<BatchInventoryResponeDTO>>(items);
        }

        public async Task<BatchInventoryResponeDTO?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);

            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy kho hàng có ID {id}");

            return _mapper.Map<BatchInventoryResponeDTO>(entity);
        }

        public async Task<BatchInventoryResponeDTO> CreateAsync(BatchInventoryCreateDTO dto, CancellationToken ct = default)
        {
            var product = await _productRepo.GetProductByIdAsync(dto.ProductId, true, ct);
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm có ID {dto.ProductId}");


            if (dto.VendorId.HasValue)
            {
                var vendorUser = await _userRepo.GetVerifiedAndActiveUserByIdAsync(dto.VendorId.Value, ct);


                if (vendorUser.Role != UserRole.Vendor)
                    throw new ArgumentException($"Người dùng với ID {dto.VendorId.Value} không phải là nhà cung cấp.");
            }

            var category = product.Category;
            bool serialRequired = category?.SerialRequired ?? false;

            if (serialRequired)
            {
                if (dto.Quantity <= 0)
                    throw new ArgumentException("Sản phẩm yêu cầu serial: Quantity phải > 0.");

                if (string.IsNullOrWhiteSpace(dto.LotNumber))
                    throw new ArgumentException("Sản phẩm yêu cầu serial: LotNumber bắt buộc.");
            }

            var entity = _mapper.Map<BatchInventory>(dto);
            var created = await _repo.CreateAsync(entity, ct);

            await UpdateProductStock(dto.ProductId, ct);

            return _mapper.Map<BatchInventoryResponeDTO>(created);
        }

        public async Task<BatchInventoryResponeDTO> UpdateAsync(BatchInventoryUpdateDTO dto, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(dto.Id, ct);
            if (existing == null)
                throw new KeyNotFoundException($"Không tìm thấy kho hàng có ID {dto.Id}");

            var product = await _productRepo.GetProductByIdAsync(dto.ProductId, useNoTracking: true, ct);
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm có ID {dto.ProductId}");

            var category = product.Category;
            bool serialRequired = category?.SerialRequired ?? false;

            if (serialRequired)
            {
                if (dto.Quantity <= 0)
                    throw new ArgumentException("Sản phẩm yêu cầu serial: Quantity phải > 0.");

                if (string.IsNullOrWhiteSpace(dto.LotNumber))
                    throw new ArgumentException("Sản phẩm yêu cầu serial: LotNumber bắt buộc.");
            }

            if (dto.VendorId.HasValue)
            {
                var vendorUser = await _userRepo.GetUserWithAddressesByIdAsync(dto.VendorId.Value, ct);

                if (vendorUser == null)
                    throw new ArgumentException($"Không tìm thấy người dùng có ID {dto.VendorId.Value}");

                if (vendorUser.Role != UserRole.Vendor)
                    throw new ArgumentException($"User ID {dto.VendorId.Value} is not a vendor.");
            }

         


            _mapper.Map(dto, existing);

            await _repo.UpdateAsync(existing, ct);
            await UpdateProductStock(existing.ProductId, ct);


            return _mapper.Map<BatchInventoryResponeDTO>(existing);
        }

        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var exists = await _repo.GetByIdAsync(id, ct);
            if (exists == null)
                throw new KeyNotFoundException($"Không tìm thấy kho hàng có ID {id}");

            await _repo.DeleteAsync(id, ct);
            await UpdateProductStock(exists.ProductId, ct);
        }

        public async Task QualityCheckAsync(ulong id, BatchInventoryQualityCheckDTO dto, CancellationToken ct = default)
        {
            var exists = await _repo.GetByIdAsync(id, ct);

            if (exists == null)
                throw new KeyNotFoundException($"Không tìm thấy kho hàng có ID {id}");

            await _repo.QualityCheckAsync(id, dto.Status, dto.QualityCheckedByUserId, dto.Notes, ct);

        }

        public async Task<IEnumerable<ProductSerial>> GetAllSerialsByProductIdAsync(ulong productId, CancellationToken ct = default)
        {
            return await _serialRepo.GetAllByProductIdAsync(productId, ct);
        }

        public async Task<IEnumerable<ProductSerial>> GetAllSerialsByBatchIdAsync(ulong batchId, CancellationToken ct = default)
        {
            return await _serialRepo.GetAllByBatchIdAsync(batchId, ct);
        }

        public async Task UpdateSerialStatusAsync(ulong serialId, ProductSerialStatus newStatus, CancellationToken ct = default)
        {
            var serial = await _serialRepo.GetByIdAsync(serialId, ct);
            if (serial == null)
                throw new KeyNotFoundException($"Không tìm thấy serial ID {serialId}");

            serial.Status = newStatus;
            serial.UpdatedAt = DateTime.UtcNow;

            await _serialRepo.UpdateAsync(serial, ct);
        }



        private async Task UpdateProductStock(ulong productId, CancellationToken ct)
        {
            var batches = await _repo.GetByProductIdAsync(productId, 1, int.MaxValue, ct);
            var total = batches.Sum(x => x.Quantity);

            await _productRepo.UpdateStockAsync(productId, total, ct);
        }
    }
}
