using AutoMapper;
using BLL.DTO.ProductCategory;
using BLL.Helpers;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IMapper _mapper;
        public ProductCategoryService(IProductCategoryRepository productCategoryRepository, IMapper mapper)
        {
            _productCategoryRepository = productCategoryRepository;
            _mapper = mapper;
        }

        public async Task<ProductCategoryResponseDTO> CreateProductCategoryAsync( ProductCategoryCreateDTO dto, CancellationToken cancellationToken = default)
        {
            string slug = Utils.ConvertToSlug(dto.Name); 

            var productCategory = _mapper.Map<ProductCategory>(dto);
            productCategory.Slug = slug;
            var createdProductCategory = await _productCategoryRepository.CreateProductCategoryAsync(productCategory, cancellationToken);

            var response = _mapper.Map<ProductCategoryResponseDTO>(createdProductCategory);
            return response;

        }

        public async Task<IReadOnlyList<ProductCategoryResponseDTO>> GetAllProductCategoryAsync(CancellationToken cancellationToken = default)
        {
            var list = await _productCategoryRepository.GetAllProductCategoryAsync(cancellationToken);

            var response = _mapper.Map<IReadOnlyList<ProductCategoryResponseDTO>>(list);
            return response;
        }


        public async Task<ProductCategoryResponseDTO?> GetProductCategoryByFarmIdAsync(ulong id, CancellationToken cancellationToken = default)
        {
            var productCategory = await _productCategoryRepository.GetProductCategoryByIdAsync(id, useNoTracking: true, cancellationToken);
            if (productCategory == null)
                return null;
            var response = _mapper.Map<ProductCategoryResponseDTO>(productCategory);
            return response;
        }

        public async Task<ProductCategoryResponseDTO> UpdateProductCategoryAsync(ulong id, ProductCategoryUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            var productCategory = await _productCategoryRepository.GetProductCategoryByIdAsync(id, useNoTracking: false, cancellationToken);
            if (productCategory == null)
                throw new KeyNotFoundException("Không tìm thấy danh mục sản phẩm");
            _mapper.Map(dto, productCategory);
            var result = await _productCategoryRepository.UpdateProductCategoryAsync(productCategory, cancellationToken);
            var response = _mapper.Map<ProductCategoryResponseDTO>(result);
            if (response == null)
                throw new InvalidOperationException("Cập nhật danh mục sản phẩm thất bại");
            return response;
        }
    }
}
