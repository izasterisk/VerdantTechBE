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

        public async Task<ProductCategoryResponseDTO> CreateProductCategoryAsync(ProductCategoryCreateDTO dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
            if (await _productCategoryRepository.IsCategoryNameNotUniqueAsync(dto.Name, cancellationToken))
                throw new ArgumentException("Tên danh mục sản phẩm đã tồn tại");
            if (dto.ParentId != null)
            {
                var productCategoryParent = await _productCategoryRepository.GetProductCategoryByIdAsync(dto.ParentId.Value, useNoTracking: false, cancellationToken);
                if (productCategoryParent == null || productCategoryParent.IsActive == false)
                    throw new KeyNotFoundException("Không tìm thấy danh mục cha hoặc đã bị xóa.");
            }
            string slug = Utils.GenerateSlug(dto.Name);
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


        public async Task<ProductCategoryResponseDTO?> GetProductCategoryByIdAsync(ulong id, CancellationToken cancellationToken = default)
        {
            var productCategory = await _productCategoryRepository.GetProductCategoryByIdAsync(id, useNoTracking: true,
                cancellationToken: cancellationToken
            );
            return productCategory == null ? null : _mapper.Map<ProductCategoryResponseDTO>(productCategory);
        }

        public async Task<ProductCategoryResponseDTO> UpdateProductCategoryAsync(ulong id, ProductCategoryUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            var productCategory = await _productCategoryRepository.GetProductCategoryByIdAsync(id, useNoTracking: false, cancellationToken);
            if (productCategory == null || productCategory.IsActive == false)
                throw new KeyNotFoundException("Không tìm thấy danh mục hoặc đã bị xóa.");
            if (dto.Name != null)
            {
                if(await _productCategoryRepository.IsCategoryNameNotUniqueAsync(dto.Name, cancellationToken))
                    throw new ArgumentException("Tên danh mục sản phẩm đã tồn tại");
                productCategory.Slug = Utils.GenerateSlug(dto.Name);
            }
            if (dto.ParentId.HasValue)
            {
                if (await _productCategoryRepository.IsCategoryAlreadyAFatherAsync(productCategory.Id, cancellationToken))
                {
                    throw new ArgumentException("Category này đã là một category cha, không thể làm category con của một category khác.");
                }
                if (dto.ParentId == productCategory.Id)
                {
                    throw new ArgumentException("Category không thể có parent ID giống với chính ID của nó.");
                }
                if (!await _productCategoryRepository.IsParentIdExistsAsync(dto.ParentId.Value, cancellationToken))
                {
                    throw new ArgumentException("Parent Category không tồn tại");
                }
            }
            
            _mapper.Map(dto, productCategory);
            var result = await _productCategoryRepository.UpdateProductCategoryAsync(productCategory, cancellationToken);
            var response = _mapper.Map<ProductCategoryResponseDTO>(result);
            return response;
        }
    }
}
