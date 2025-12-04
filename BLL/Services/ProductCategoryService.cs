using AutoMapper;
using BLL.DTO;
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
        
        //CATEGORY CHỈ CÓ THỂ CÓ TỐI ĐA 1 CẤP CHA
        //Ví dụ: Máy móc -> Máy cày. Nhưng không thể thêm một category là con của Máy cày nữa.
        
        public async Task<ProductCategoryResponseDTO> CreateProductCategoryAsync(ProductCategoryCreateDTO dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
            if (await _productCategoryRepository.IsCategoryNameNotUniqueAsync(dto.Name, cancellationToken))
                throw new ArgumentException("Tên danh mục sản phẩm đã tồn tại");
            if (dto.ParentId != null)
            {
                if (await _productCategoryRepository.IsCategoryAlreadySonsAsync(dto.ParentId.Value, cancellationToken))
                    throw new KeyNotFoundException("Danh mục được chỉ định làm cha đang là danh mục con của một mục khác.");
            }
            string slug = Utils.GenerateSlug(dto.Name);
            var productCategory = _mapper.Map<ProductCategory>(dto);
            productCategory.Slug = slug;
            var createdProductCategory = await _productCategoryRepository.CreateProductCategoryAsync(productCategory, cancellationToken);

            var response = _mapper.Map<ProductCategoryResponseDTO>(createdProductCategory);
            return response;

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
            if (dto.ParentId != null)
            {
                if (await _productCategoryRepository.IsCategoryAlreadySonsAsync(dto.ParentId.Value, cancellationToken))
                    throw new KeyNotFoundException("Danh mục được chỉ định làm cha đang là danh mục con của một mục khác.");
                if (dto.ParentId == productCategory.Id)
                    throw new ArgumentException("Category không thể có parent ID giống với chính ID của nó.");
            }
            _mapper.Map(dto, productCategory);
            var result = await _productCategoryRepository.UpdateProductCategoryAsync(productCategory, cancellationToken);
            var response = _mapper.Map<ProductCategoryResponseDTO>(result);
            return response;
        }

        public async Task<PagedResponse<ProductCategoryResponseDTO>> GetAllProductCategoryAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var (items, total) = await _productCategoryRepository.GetAllProductCategoryAsync(page, pageSize, cancellationToken);
            var list = _mapper.Map<List<ProductCategoryResponseDTO>>(items);
            return ToPaged(list, total, page, pageSize);
        }

        private static PagedResponse<T> ToPaged<T>(List<T> items, int totalRecords, int page, int pageSize)
        {
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            return new PagedResponse<T>
            {
                Data = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }


        public async Task<ProductCategoryResponseDTO?> GetProductCategoryByIdAsync(ulong id, CancellationToken cancellationToken = default)
        {
            var productCategory = await _productCategoryRepository.GetProductCategoryByIdAsync(id, useNoTracking: true,
                cancellationToken: cancellationToken
            );
            return productCategory == null ? null : _mapper.Map<ProductCategoryResponseDTO>(productCategory);
        }
    }
}
