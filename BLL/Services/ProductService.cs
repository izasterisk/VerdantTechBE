using AutoMapper;
using BLL.DTO.Product;
using BLL.Interfaces;
using DAL.IRepository;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productService, IMapper mapper)
        { 
            _productRepository = productService;
            _mapper = mapper;
        }
        //public Task<ProductResponseDTO> CreateProductAsync(ProductCreateDTO dto, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}

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
    }
}
