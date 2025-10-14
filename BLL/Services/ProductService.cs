using AutoMapper;
using BLL.DTO;
using BLL.DTO.Product;
using BLL.DTO.ProductCategory;
using BLL.DTO.ProductRegistration;
using BLL.DTO.User;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;
using DAL.Repository;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductRegistrationRepository _productRegistrationRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productService,IProductRegistrationRepository productRegistrationRepository,IProductCategoryRepository productCategoryRepository, IMapper mapper)
        { 
            _productRegistrationRepository = productRegistrationRepository;
            _productRepository = productService;
            _productCategoryRepository = productCategoryRepository;
            _mapper = mapper;
        }
        //public Task<ProductResponseDTO> CreateProductAsync(ProductCreateDTO dto, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}
        public async Task<ProductRegistrationReponseDTO> ProductRegistrationAsync(ulong currentUserId, ProductRegistrationCreateDTO requestDTO, CancellationToken cancellationToken = default)
        {
            
            var productCategory = await _productCategoryRepository.GetProductCategoryByIdAsync(requestDTO.CategoryId, useNoTracking: true, cancellationToken);
            if (productCategory == null || !productCategory.IsActive)
                throw new KeyNotFoundException("Danh mục sản phẩm không tồn tại hoặc không hợp lệ");
            if(currentUserId == 0) 
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc không hợp lệ");
            var productEntity = _mapper.Map<ProductRegistration>(requestDTO);
            productEntity.CreatedAt = DateTime.UtcNow;
            productEntity.VendorId = currentUserId;

            var createdProduct = await _productRegistrationRepository.CreateProductAsync(productEntity, cancellationToken);
            var responseDTO = _mapper.Map<ProductRegistrationReponseDTO>(createdProduct);
            return responseDTO;
        }
        public async Task<IReadOnlyList<ProductResponseDTO>> GetAllProductAsync(CancellationToken cancellationToken = default)
        {
            var list = await _productRepository.GetAllProductAsync(cancellationToken);
            var response = _mapper.Map<IReadOnlyList<ProductResponseDTO>>(list);
            return response;
        }

        public async Task<IReadOnlyList<ProductResponseDTO?>> GetAllProductByCategoryIdAsync(ulong id, CancellationToken cancellationToken = default)
        {
            var list = await _productRepository.GetAllProductByCategoryIdAsync(id, useNoTracking: true, cancellationToken);
            var response = _mapper.Map<IReadOnlyList<ProductResponseDTO?>>(list);
            return response;
        }

        public async Task<ProductResponseDTO?> GetProductByIdAsync(ulong id, CancellationToken cancellationToken = default)
        {
         
            var product = await _productRepository.GetProductByIdAsync(id, useNoTracking: true, cancellationToken);
            if (product == null)
                return null;
            var response = _mapper.Map<ProductResponseDTO>(product);
            return response;

        }

        public async Task<ProductResponseDTO> UpdateProductAsync(ulong id, ProductUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetProductByIdAsync(id, useNoTracking: false, cancellationToken);
            if (product == null)
                throw new KeyNotFoundException("Không tìm thấy sản phẩm");

            var updatedProduct = _mapper.Map(dto, product);

            var result = await _productRepository.UpdateProductAsync(updatedProduct, cancellationToken);

            var response = _mapper.Map<ProductResponseDTO>(result);
            return response;
        }

        public async Task<IReadOnlyList<ProductRegistrationReponseDTO?>> GetAllProductByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default)
        {

            if (vendorId == 0)
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc không hợp lệ");
            var list =  await _productRegistrationRepository.GetProductRegistrationByVendorIdAsync(vendorId, useNoTracking: true, cancellationToken);
            var response = _mapper.Map<IReadOnlyList<ProductRegistrationReponseDTO?>>(list);
            return response;
        }
        public async Task<PagedResponse<ProductRegistrationReponseDTO>> GetAllProductRegisterAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var (product, totalCount) = await _productRegistrationRepository.GetAllProductRegistrationAsync(page, pageSize, cancellationToken);
            var userDtos = _mapper.Map<List<ProductRegistrationReponseDTO>>(product);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResponse<ProductRegistrationReponseDTO>
            {
                Data = userDtos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalCount,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
    }
}
