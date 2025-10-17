//using AutoMapper;
//using BLL.DTO.ProductRegistration;
//using BLL.Interfaces;
//using DAL.Data;
//using DAL.Data.Models;
//using DAL.IRepository;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BLL.Helpers;

//namespace BLL.Services
//{

//        public class StaffService : IStaffService
//        {
//            private readonly IProductRegistrationRepository _productRegistrationRepository;
//            private readonly IProductRepository _productRepository;
//            private readonly IMapper _mapper;

//            public StaffService(IProductRegistrationRepository productRegistrationRepository, IMapper mapper, IProductRepository productRepository)
//            {
//                _productRegistrationRepository = productRegistrationRepository;
//                _mapper = mapper;
//                _productRepository = productRepository;
//            }

//            // APPROVE (không cần DTO nếu không chỉnh sửa)
//            public async Task<ProductRegistrationReponseDTO> ApproveProductRegistrationAsync(
//                ulong id,
//                ulong staffId,
//                CancellationToken cancellationToken = default)
//            {
//                var reg = await _productRegistrationRepository.GetProductRegistrationByIdAsync(id);
//                if (reg is null) throw new KeyNotFoundException("Sản phẩm không tìm thấy");
//                if (reg.Status == ProductRegistrationStatus.Approved)
//                    throw new InvalidOperationException("Sản phẩm đã được chấp thuận từ trước.");
//                if (reg.Status == ProductRegistrationStatus.Rejected)
//                    throw new InvalidOperationException("Sản phẩm đã bị từ chối từ trước.");

//                // Set nghiệp vụ approve
//                reg.Status = ProductRegistrationStatus.Approved;
//                reg.ApprovedBy = staffId;
//                reg.ApprovedAt = DateTime.UtcNow;
//                reg.RejectionReason = null;
//                reg.UpdatedAt = DateTime.UtcNow;

//                // Map sang Product
//                var product = _mapper.Map<Product>(reg);

//                // Tạo slug (và nên đảm bảo unique trong repo nếu có)
//                var baseSlug = reg.ProposedProductName ?? reg.ProposedProductCode ?? string.Empty;
//                product.Slug = Utils.GenerateSlug(baseSlug);
//                product.CreatedAt = DateTime.UtcNow;   // nếu cột NOT NULL
//                product.UpdatedAt = DateTime.UtcNow;

//                // Transaction cho nhất quán (nếu repo bạn hỗ trợ; nếu chưa có, gọi tuần tự như dưới)
//                await _productRegistrationRepository.UpdateProductRegistrationAsync(reg, cancellationToken);
//                await _productRepository.CreateProductAsync(product, cancellationToken);

//                return _mapper.Map<ProductRegistrationReponseDTO>(reg);
//            }

//            // REJECT
//            public async Task<ProductRegistrationReponseDTO> RejectProductRegistrationAsync(
//                ulong id,
//                ulong staffId,
//                string reason,
//                CancellationToken cancellationToken = default)
//            {
//                var reg = await _productRegistrationRepository.GetProductRegistrationByIdAsync(id);
//                if (reg is null) throw new KeyNotFoundException("Sản phẩm không tìm thấy");
//                if (reg.Status == ProductRegistrationStatus.Approved)
//                    throw new InvalidOperationException("Sản phẩm đã được chấp thuận từ trước.");
//                if (reg.Status == ProductRegistrationStatus.Rejected)
//                    throw new InvalidOperationException("Sản phẩm đã bị từ chối từ trước.");

//                reg.Status = ProductRegistrationStatus.Rejected;
//                reg.RejectionReason = reason?.Trim();
//                reg.UpdatedAt = DateTime.UtcNow;

//                // (Tuỳ bạn) có thể lưu StaffId vào một trường audit khác nếu schema có (ví dụ RejectedBy),
//                // hiện tại giữ nguyên ApprovedBy/ApprovedAt = null khi từ chối

//                await _productRegistrationRepository.UpdateProductRegistrationAsync(reg, cancellationToken);

//                return _mapper.Map<ProductRegistrationReponseDTO>(reg);
//            }
//        }

//    }

