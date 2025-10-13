using AutoMapper;
using BLL.DTO.ProductCertificate;
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
    public class ProductCertificateService : IProductCertificateService
    {
        private readonly IProductCertificateRepository _productCertificateRepository;
        private readonly IMapper _mapper;
        public ProductCertificateService(IProductCertificateRepository productCertificateRepository, IMapper mapper)
        {
            _productCertificateRepository = productCertificateRepository;
            _mapper = mapper;
        }
        public async Task<ProductCertificateResponseDTO> CreateProductCertificateAsync(ProductCertificateCreateDTO productCertificate, CancellationToken cancellationToken = default)
        {
            if (productCertificate == null)
            {
                throw new ArgumentNullException(nameof(productCertificate));
            }

            var entity = _mapper.Map<ProductCertificate>(productCertificate);
            entity.CreatedAt = DateTime.UtcNow;

            var createdEntity = await _productCertificateRepository.CreateProductCertificateAsync(entity, cancellationToken);
            var responseDTO = _mapper.Map<ProductCertificateResponseDTO>(createdEntity);

            return responseDTO;
        }

        public async Task<IReadOnlyList<ProductCertificateResponseDTO?>> GetAllProductCertificatesByProductIdAsync(ulong productId, CancellationToken cancellationToken = default)
        {
            if (productId == 0)
            {
                throw new ArgumentException("Không tìm thấy ProductID", nameof(productId));
            }
            var list = await _productCertificateRepository.GetAllProductCertificatesByProductIdAsync(productId, cancellationToken);
            var response = _mapper.Map<IReadOnlyList<ProductCertificateResponseDTO?>>(list);
            return response;


        }

        public async Task<ProductCertificateResponseDTO?> GetProductCertificateByIdAsync(ulong productCertificateId, CancellationToken cancellationToken = default)
        {
            if (productCertificateId == 0)
            {
                throw new ArgumentException("Không tìm thấy ProductCertificateID", nameof(productCertificateId));
            }
            var productCertificate = await _productCertificateRepository.GetProductCertificateByIdAsync(productCertificateId, cancellationToken);
            var response = _mapper.Map<ProductCertificateResponseDTO?>(productCertificate);
            return response;

        }

        public async Task<ProductCertificateResponseDTO> UpdateProductCertificateAsync(ProductCertificateUpdateDTO productCertificate, CancellationToken cancellationToken = default)
        {
            if(productCertificate == null)
            {
                throw new ArgumentNullException(nameof(productCertificate));
            }

            var entity = _mapper.Map<ProductCertificate>(productCertificate);
            entity.UpdatedAt = DateTime.UtcNow;
            var update = await _productCertificateRepository.UpdateProductCertificateWithTransactionAsync(entity, cancellationToken);
            var response = _mapper.Map<ProductCertificateResponseDTO>(update);
            return response;
        }
    }
}
