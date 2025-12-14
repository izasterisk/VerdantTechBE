using AutoMapper;
using BLL.DTO.BatchInventory;
using BLL.Helpers.BatchInventoryHelper;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

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

            string sku;
            do
            {
                sku = BatchInventoryHelper.GenerateSku(dto.ProductId);
            } while (await _repo.SkuExistsAsync(sku, null, ct));

            // Validate BatchNumber - format validation và check duplicate
            if (string.IsNullOrWhiteSpace(dto.BatchNumber))
                throw new ArgumentException("Số lô (BatchNumber) không được để trống.");

            dto.BatchNumber = dto.BatchNumber.Trim();

            // Validate BatchNumber format (chỉ chứa chữ cái, số, dấu gạch ngang, gạch dưới, khoảng trắng)
            if (!dto.BatchNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
                throw new ArgumentException("Số lô (BatchNumber) chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

            if (dto.BatchNumber.Length > 100)
                throw new ArgumentException("Số lô (BatchNumber) không được vượt quá 100 ký tự.");

            // Check BatchNumber duplicate - không được trùng
            if (await _repo.BatchNumberExistsAsync(dto.BatchNumber, null, ct))
                throw new ArgumentException($"Số lô (BatchNumber) '{dto.BatchNumber}' đã tồn tại trong hệ thống. Vui lòng sử dụng số lô khác.");

            // Validate LotNumber - có thể trùng, chỉ validate format
            if (string.IsNullOrWhiteSpace(dto.LotNumber))
                throw new ArgumentException("Mã số lô (LotNumber) không được để trống.");

            dto.LotNumber = dto.LotNumber.Trim();

            // Validate LotNumber format
            if (!dto.LotNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
                throw new ArgumentException("Mã số lô (LotNumber) chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

            if (dto.LotNumber.Length > 100)
                throw new ArgumentException("Mã số lô (LotNumber) không được vượt quá 100 ký tự.");

            // Note: LotNumber có thể trùng, không cần check duplicate

            var category = product.Category;
            bool serialRequired = category?.SerialRequired ?? false;
            List<string>? validSerials = null;

            if (serialRequired)
            {
                if (dto.Quantity <= 0)
                    throw new ArgumentException("Sản phẩm yêu cầu serial: Quantity phải > 0.");

                if (string.IsNullOrWhiteSpace(dto.LotNumber))
                    throw new ArgumentException("Sản phẩm yêu cầu serial: LotNumber bắt buộc.");

                // Validate serial numbers
                if (dto.SerialNumbers == null || dto.SerialNumbers.Count == 0)
                    throw new ArgumentException("Sản phẩm yêu cầu serial: Vui lòng nhập danh sách số serial.");

                // Remove empty/whitespace serials
                validSerials = dto.SerialNumbers
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToList();

                if (validSerials.Count != dto.Quantity)
                    throw new ArgumentException($"Số lượng serial ({validSerials.Count}) phải bằng số lượng sản phẩm ({dto.Quantity}).");

                // Check for duplicates within input
                var duplicateSerials = validSerials
                    .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateSerials.Any())
                    throw new ArgumentException($"Có serial trùng lặp trong danh sách nhập: {string.Join(", ", duplicateSerials)}");

                // Check for existing serials in database
                var existingSerials = await _serialRepo.GetExistingSerialNumbersAsync(validSerials, ct);

                if (existingSerials.Any())
                    throw new ArgumentException($"Các serial sau đã tồn tại trong hệ thống: {string.Join(", ", existingSerials)}");
            }
            else
            {
                // Non-serial products should not have serial numbers
                if (dto.SerialNumbers != null && dto.SerialNumbers.Any(s => !string.IsNullOrWhiteSpace(s)))
                    throw new ArgumentException("Sản phẩm này không yêu cầu serial. Vui lòng không nhập số serial.");
            }

            var entity = _mapper.Map<BatchInventory>(dto);
            // Set SKU (tự động tạo hoặc từ DTO)
            entity.Sku = sku;
            var created = await _repo.CreateAsync(entity, ct);

            // Create ProductSerial records if serials are provided
            if (serialRequired && validSerials != null && validSerials.Any())
            {
                var serials = new List<ProductSerial>();
                var now = DateTime.UtcNow;

                foreach (var serialNumber in validSerials)
                {
                    serials.Add(new ProductSerial
                    {
                        BatchInventoryId = created.Id,
                        ProductId = created.ProductId,
                        SerialNumber = serialNumber,
                        Status = ProductSerialStatus.Stock,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                await _serialRepo.CreateRangeAsync(serials, ct);
            }

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

            // Validate SKU (Mã nhập kho) - không được trùng
            if (string.IsNullOrWhiteSpace(dto.Sku))
                throw new ArgumentException("Mã nhập kho (SKU) không được để trống.");

            dto.Sku = dto.Sku.Trim();

            if (dto.Sku.Length > 100)
                throw new ArgumentException("Mã nhập kho (SKU) không được vượt quá 100 ký tự.");

            // Check SKU duplicate - không được trùng (exclude current record)
            if (await _repo.SkuExistsAsync(dto.Sku, dto.Id, ct))
                throw new ArgumentException($"Mã nhập kho (SKU) '{dto.Sku}' đã tồn tại trong hệ thống. Vui lòng sử dụng mã khác.");

            // Validate BatchNumber - format validation và check duplicate
            if (string.IsNullOrWhiteSpace(dto.BatchNumber))
                throw new ArgumentException("Số lô (BatchNumber) không được để trống.");

            dto.BatchNumber = dto.BatchNumber.Trim();

            if (!dto.BatchNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
                throw new ArgumentException("Số lô (BatchNumber) chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

            if (dto.BatchNumber.Length > 100)
                throw new ArgumentException("Số lô (BatchNumber) không được vượt quá 100 ký tự.");

            // Check BatchNumber duplicate - không được trùng (exclude current record)
            if (await _repo.BatchNumberExistsAsync(dto.BatchNumber, dto.Id, ct))
                throw new ArgumentException($"Số lô (BatchNumber) '{dto.BatchNumber}' đã tồn tại trong hệ thống. Vui lòng sử dụng số lô khác.");

            // Validate LotNumber - có thể trùng, chỉ validate format
            if (string.IsNullOrWhiteSpace(dto.LotNumber))
                throw new ArgumentException("Mã số lô (LotNumber) không được để trống.");

            dto.LotNumber = dto.LotNumber.Trim();

            if (!dto.LotNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
                throw new ArgumentException("Mã số lô (LotNumber) chỉ được chứa chữ cái, số, dấu gạch ngang (-), gạch dưới (_) và khoảng trắng.");

            if (dto.LotNumber.Length > 100)
                throw new ArgumentException("Mã số lô (LotNumber) không được vượt quá 100 ký tự.");

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
