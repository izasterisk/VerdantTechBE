using AutoMapper;
using BLL.DTO.Address;
using BLL.DTO.BatchInventory;
using BLL.DTO.Cart;
using BLL.DTO.CO2;
using BLL.DTO.Courier;
using BLL.DTO.FarmProfile;
using BLL.DTO.ForumCategory;
using BLL.DTO.MediaLink;
using BLL.DTO.Notification;
using BLL.DTO.Order;
using BLL.DTO.Product;
using BLL.DTO.ProductCategory;
using BLL.DTO.ProductCertificate;
using BLL.DTO.ProductRegistration;
using BLL.DTO.ProductReview;
using BLL.DTO.Request;
using BLL.DTO.Transaction;
using BLL.DTO.User;
using BLL.DTO.UserBankAccount;
using BLL.DTO.VendorCertificate;
using BLL.DTO.Wallet;
using BLL.Services.Payment;
using DAL.Data.Models;
using System.Numerics;
using BLL.DTO.ChatbotConversations;
using ProductResponseDTO = BLL.DTO.Order.ProductResponseDTO;
using BLL.DTO.ForumPost;
using BLL.DTO.ForumComment;

namespace BLL.Helpers
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            // ===================== USER MAPPINGS =====================
            CreateMap<UserCreateDTO, User>().ReverseMap();
            CreateMap<StaffCreateDTO, User>().ReverseMap();
            CreateMap<UserUpdateDTO, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<User, UserResponseDTO>().ReverseMap();

            CreateMap<UserAddressCreateDTO, Address>().ReverseMap();
            CreateMap<UserAddressUpdateDTO, Address>().ReverseMap();
            CreateMap<UserAddressUpdateDTO, UserAddress>().ReverseMap();
            
            // ===================== NOTIFICATION MAPPINGS =====================
            CreateMap<NotificationResponseDTO, Notification>().ReverseMap();

            // ===================== USER BANK ACCOUNT MAPPINGS =====================
            CreateMap<UserBankAccountCreateDTO, UserBankAccount>().ReverseMap();
            CreateMap<UserBankAccount, UserBankAccountResponseDTO>().ReverseMap();

            // ===================== WALLET MAPPINGS =====================
            CreateMap<WalletResponseDTO, Wallet>().ReverseMap();
            CreateMap<WalletCashoutRequestResponseDTO, UserBankAccount>().ReverseMap();
            CreateMap<Cashout, WalletCashoutRequestResponseDTO>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.BankAccount.User))
                .ForMember(d => d.ProcessedBy, o => o.MapFrom(s => s.ProcessedByNavigation));
            CreateMap<WalletCashoutRequestCreateDTO, Cashout>().ReverseMap();
            
            CreateMap<Cashout, WalletCashoutResponseDTO>()
                .ForMember(dest => dest.ProcessedBy, opt => opt.MapFrom(src => src.ProcessedByNavigation));            
            CreateMap<Transaction, WalletTransactionResponseDTO>();
            
            // ===================== FARM PROFILE =====================
            CreateMap<FarmProfileCreateDto, FarmProfile>().ReverseMap();
            CreateMap<FarmProfile, FarmProfileResponseDTO>().ReverseMap();
            CreateMap<FarmProfileUpdateDTO, FarmProfile>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CropsCreateDTO, Crop>().ReverseMap();
            CreateMap<CropsDTO, Crop>().ReverseMap();
            CreateMap<CropsUpdateDTO, Crop>().ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // ===================== ADDRESS =====================
            CreateMap<Address, AddressResponseDTO>().ReverseMap();
            CreateMap<UserAddress, AddressResponseDTO>()
                .IncludeMembers(src => src.Address)
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

            CreateMap<FarmProfileCreateDto, Address>().ReverseMap();
            CreateMap<FarmProfileUpdateDTO, Address>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ===================== CO2 FOOTPRINT =====================
            CreateMap<Fertilizer, CO2FootprintCreateDTO>().ReverseMap();
            CreateMap<EnvironmentalDatum, CO2FootprintCreateDTO>().ReverseMap();
            CreateMap<EnergyUsage, CO2FootprintCreateDTO>().ReverseMap();

            CreateMap<EnvironmentalDatum, CO2FootprintResponseDTO>()
                .ForMember(dest => dest.EnergyUsage, opt => opt.MapFrom(src => src.EnergyUsage))
                .ForMember(dest => dest.Fertilizer, opt => opt.MapFrom(src => src.Fertilizer));

            CreateMap<EnergyUsageDTO, EnergyUsage>().ReverseMap();
            CreateMap<FertilizerDTO, Fertilizer>().ReverseMap();

            // ===================== PRODUCT CATEGORY =====================
            CreateMap<ProductCategoryCreateDTO, ProductCategory>().ReverseMap();
            CreateMap<ProductCategoryUpdateDTO, ProductCategory>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ProductCategory, ProductCategoryResponseDTO>().ReverseMap();

            // ===================== MEDIA LINK =====================
            CreateMap<MediaLink, MediaLinkItemDTO>()
                .ForMember(d => d.ImagePublicId, o => o.MapFrom(s => s.ImagePublicId))
                .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl))
                .ForMember(d => d.SortOrder, o => o.MapFrom(s => s.SortOrder))
                .ForMember(d => d.Purpose, o => o.MapFrom(s => s.Purpose.ToString().ToLowerInvariant()))
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id));

            CreateMap<MediaLink, ImagesDTO>().ReverseMap();
            CreateMap<MediaLink, ProductImageResponseDTO>().ReverseMap();


            // ===================== FORUM CATEGORY =====================
            CreateMap<ForumCategoryCreateDTO, ForumCategory>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<ForumCategoryUpdateDTO, ForumCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ForumCategory, ForumCategoryResponseDto>();

            // ===================== FORUM POST =====================
            CreateMap<ContentBlockDTO, ContentBlock>().ReverseMap();

            CreateMap<ForumPost, ForumPostResponseDTO>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Images, o => o.Ignore())   
                .ForMember(d => d.Comments, o => o.Ignore());

            // ===================== FORUM COMMENT =====================
            CreateMap<ForumComment, ForumCommentResponseDTO>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
                .ForMember(d => d.Replies, o => o.Ignore());


            // ===================== CART =====================
            CreateMap<CartDTO, CartItem>().ReverseMap();
            CreateMap<DAL.Data.Models.Cart, CartResponseDTO>()
                .ForMember(d => d.UserInfo, o => o.MapFrom(s => s.Customer));

            CreateMap<CartItem, CartItemDTO>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.Slug, o => o.MapFrom(s => s.Product.Slug))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Product.Description))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.Product.UnitPrice))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.Product.IsActive))
                .ForMember(d => d.SoldCount, o => o.MapFrom(s => s.Product.SoldCount))
                .ForMember(d => d.RatingAverage, o => o.MapFrom(s => s.Product.RatingAverage));

            //// ===================== PRODUCT REGISTRATION =====================
            //CreateMap<ProductRegistration, ProductRegistrationReponseDTO>()
            //    // basic info
            //    .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            //    .ForMember(d => d.EnergyEfficiencyRating,
            //        o => o.MapFrom(s => s.EnergyEfficiencyRating.HasValue
            //                            ? s.EnergyEfficiencyRating.Value.ToString()
            //                            : null))
            //    // manual file URLs
            //    .ForMember(d => d.ManualUrl, o => o.MapFrom(s => s.ManualUrls))
            //    .ForMember(d => d.ManualPublicUrl, o => o.MapFrom(s => s.PublicUrl))
            //    // media
            //    .ForMember(d => d.ProductImages, o => o.MapFrom(s => s.ProductImages))
            //    .ForMember(d => d.CertificateFiles, o => o.MapFrom(s => s.CertificateFiles))
            //    // specs & dimensions
            //    .ForMember(d => d.DimensionsCm,
            //        o => o.MapFrom(s => s.DimensionsCm != null
            //            ? s.DimensionsCm.ToDictionary(k => k.Key, v => (object)v.Value)
            //            : new Dictionary<string, object>()))
            //    .ForMember(d => d.Specifications,
            //        o => o.MapFrom(s => s.Specifications ?? new Dictionary<string, object>()));

            //CreateMap<ProductRegistrationCreateDTO, ProductRegistration>()
            //    .ForMember(d => d.EnergyEfficiencyRating, o => o.Ignore())
            //    //.ForMember(d => d.Specifications,
            //    //    o => o.MapFrom(s => s.Specifications ?? new Dictionary<string, object>()))
            //    .ForMember(d => d.Specifications, o => o.Ignore())
            //    .ForMember(dest => dest.DimensionsCm, opt => opt.MapFrom(src => new Dictionary<string, decimal>
            //    {
            //        { "Width", src.DimensionsCm.Width },
            //        { "Height", src.DimensionsCm.Height },
            //        { "Length", src.DimensionsCm.Length }
            //    }));

            //CreateMap<ProductRegistrationUpdateDTO, ProductRegistration>()
            //    .ForMember(d => d.EnergyEfficiencyRating, o => o.Ignore())
            //    //.ForMember(d => d.Specifications,
            //    //    o => o.MapFrom(s => s.Specifications ?? new Dictionary<string, object>()))
            //    .ForMember(d => d.Specifications, o => o.Ignore())
            //    .ForMember(dest => dest.DimensionsCm, opt => opt.MapFrom(src => new Dictionary<string, decimal>
            //    {
            //        { "Width", src.DimensionsCm.Width },
            //        { "Height", src.DimensionsCm.Height },
            //        { "Length", src.DimensionsCm.Length }
            //    }))
            //    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ProductRegistration -> ResponseDTO
            CreateMap<ProductRegistration, ProductRegistrationReponseDTO>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.EnergyEfficiencyRating,
                    o => o.MapFrom(s => s.EnergyEfficiencyRating.HasValue? s.EnergyEfficiencyRating.Value.ToString("0.0"): null))
                .ForMember(d => d.ManualUrl, o => o.MapFrom(s => s.ManualUrls))
                .ForMember(d => d.ManualPublicUrl, o => o.MapFrom(s => s.PublicUrl))
                .ForMember(d => d.Specifications, o => o.MapFrom(s => s.Specifications ?? new Dictionary<string, object>()))
                // Dimensions: cast decimal -> object để serialize đẹp
                .ForMember(d => d.DimensionsCm, o => o.MapFrom(s =>
                    s.DimensionsCm != null
                        ? s.DimensionsCm.ToDictionary(k => k.Key, v => (object)v.Value)
                        : new Dictionary<string, object>()))
                // ảnh fill bằng service Hydrate => không map trực tiếp từ entity (để tránh null)
                .ForMember(d => d.ProductImages, o => o.Ignore())
                .ForMember(d => d.CertificateFiles, o => o.Ignore());

            CreateMap<ProductCertificate, ProductCertificateResponseDTO>();


            // CreateDTO -> Entity
            CreateMap<ProductRegistrationCreateDTO, ProductRegistration>()
                .ForMember(d => d.EnergyEfficiencyRating, o => o.Ignore())         // parse bên service
                .ForMember(d => d.Specifications, o => o.Ignore())         // gán bên service sau khi binder parse
                .ForMember(d => d.DimensionsCm, o => o.MapFrom(src => new Dictionary<string, decimal>{
                    { "Width", src.DimensionsCm.Width },
                    { "Height", src.DimensionsCm.Height },
                    { "Length", src.DimensionsCm.Length }
                }));

            // UpdateDTO -> Entity
            CreateMap<ProductRegistrationUpdateDTO, ProductRegistration>()
                .ForMember(d => d.EnergyEfficiencyRating, o => o.Ignore())
                .ForMember(d => d.Specifications, o => o.Ignore())
                .ForMember(d => d.DimensionsCm, o => o.MapFrom(src => new Dictionary<string, decimal>{
                    { "Width", src.DimensionsCm.Width },
                    { "Height", src.DimensionsCm.Height },
                    { "Length", src.DimensionsCm.Length }
                }))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
           
            // map từ registration sang product
            CreateMap<ProductRegistration, Product>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.ProposedProductCode))
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProposedProductName))
                .ForMember(d => d.CategoryId, o => o.MapFrom(s => s.CategoryId))
                .ForMember(d => d.VendorId, o => o.MapFrom(s => s.VendorId))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice))
                .ForMember(d => d.EnergyEfficiencyRating, o => o.MapFrom(s => s.EnergyEfficiencyRating))
                .ForMember(d => d.Specifications, o => o.MapFrom(s => s.Specifications))
                .ForMember(d => d.ManualUrls, o => o.MapFrom(s => s.ManualUrls))
                .ForMember(d => d.WeightKg, o => o.MapFrom(s => s.WeightKg))
                .ForMember(d => d.WarrantyMonths, o => o.MapFrom(s => s.WarrantyMonths))
                .ForMember(d => d.DimensionsCm, o => o.MapFrom(s => s.DimensionsCm))
                .ForMember(d => d.Slug, o => o.Ignore())
                .ForMember(d => d.PublicUrl, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());

            // ===================== PRODUCT CERTIFICATE =====================
            CreateMap<ProductCertificateCreateDTO, ProductCertificate>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

            CreateMap<ProductCertificateUpdateDTO, ProductCertificate>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ProductCertificate, ProductCertificateResponseDTO>();
            // ===================== VENDOR CERTIFICATE =====================
            CreateMap<VendorCertificate, VendorCertificateResponseDTO>()
                .ForMember(d => d.VendorName,opt => opt.MapFrom(s => s.Vendor != null ? s.Vendor.FullName : null))
                .ForMember(d => d.VerifiedByName, opt => opt.MapFrom(s => s.VerifiedByNavigation != null ? s.VerifiedByNavigation.FullName : null))
                .ForMember(d => d.Files, opt => opt.MapFrom(s => s.MediaLinks));

            // ===================== ORDER =====================
            CreateMap<OrderPreviewCreateDTO, OrderPreviewResponseDTO>()
                .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());

            CreateMap<ProductResponseDTO, Product>().ReverseMap();
            CreateMap<ShippingDetailDTO, RateResponseDTO>().ReverseMap();
            CreateMap<OrderPreviewResponseDTO, DAL.Data.Models.Order>()
                .ForMember(dest => dest.OrderDetails, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore());

            CreateMap<DAL.Data.Models.Order, OrderResponseDTO>();
            CreateMap<OrderDetail, OrderDetailsResponseDTO>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));
            
            // ===================== PAYMENT =====================
            CreateMap<Payment, PaymentResponseDTO>().ReverseMap();
            CreateMap<Transaction, TransactionCreateDTO>().ReverseMap();

            // ===================== PRODUCT =====================
            CreateMap<BLL.DTO.Product.ProductCreateDTO, Product>().ReverseMap();
            CreateMap<BLL.DTO.Product.ProductUpdateDTO, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Product, BLL.DTO.Product.ProductResponseDTO>()
                .ForMember(d => d.EnergyEfficiencyRating,
                    o => o.MapFrom(s => s.EnergyEfficiencyRating.HasValue
                                        ? s.EnergyEfficiencyRating.Value.ToString()
                                        : null));

            CreateMap<Product, BLL.DTO.Product.ProductListItemDTO>()
                .ForMember(d => d.EnergyEfficiencyRating,
                    o => o.MapFrom(s => s.EnergyEfficiencyRating.HasValue
                                        ? s.EnergyEfficiencyRating.Value.ToString()
                                        : null));


            // ===================== BATCH INVENTORY =====================
            CreateMap<BatchInventoryCreateDTO, BatchInventory>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.QualityCheckStatus, o => o.Ignore())
                .ForMember(d => d.QualityCheckedBy, o => o.Ignore())
                .ForMember(d => d.QualityCheckedAt, o => o.Ignore());
            CreateMap<BatchInventoryQualityCheckDTO, BatchInventory>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.QualityCheckStatus, o => o.Ignore())
                .ForMember(d => d.QualityCheckedBy, o => o.Ignore())
                .ForMember(d => d.Notes, o => o.Ignore());
            CreateMap<BatchInventoryUpdateDTO, BatchInventory>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ProductId, o => o.Ignore())
                .ForMember(d => d.Sku, o => o.Ignore())
                .ForMember(d => d.BatchNumber, o => o.Ignore())
                .ForMember(d => d.LotNumber, o => o.Ignore())
                .ForMember(d => d.VendorId, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.QualityCheckStatus, o => o.Ignore())
                .ForMember(d => d.QualityCheckedBy, o => o.Ignore())
                .ForMember(d => d.QualityCheckedAt, o => o.Ignore())
                // CHỈ MAP GIÁ TRỊ KHÁC NULL (AutoMapper xử lý Optional Update)
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<BatchInventory, BatchInventoryResponeDTO>()
                .ForMember(dest => dest.ProductName, o => o.MapFrom(src => src.Product != null ? src.Product.ProductName : null))
                .ForMember(dest => dest.VendorName, o => o.MapFrom(src => src.Vendor != null ? src.Vendor.FullName : null))
                .ForMember(dest => dest.QualityCheckedByName, o => o.MapFrom(src => src.QualityCheckedByNavigation != null ? src.QualityCheckedByNavigation.FullName : null));

            // ===================== REQUEST =====================
            CreateMap<RequestCreateDTO, Request>().ReverseMap();
            CreateMap<Request, RequestResponseDTO>()
                .ForMember(d => d.ProcessedBy, o => o.MapFrom(s => s.ProcessedByNavigation));
            CreateMap<MediaLink, RequestImageDTO>().ReverseMap();

            // ===================== PRODUCT REVIEW =====================
            CreateMap<ProductReviewCreateDTO, ProductReview>().ReverseMap();
            CreateMap<ProductReview, ProductReviewResponseDTO>()
                .ForMember(d => d.Customer, o => o.MapFrom(s => s.Customer));
            CreateMap<ProductReviewUpdateDTO, ProductReview>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // ProductReview Image mappings
            CreateMap<ProductReviewImageDTO, MediaLink>().ReverseMap();
            CreateMap<MediaLink, ProductReviewImageDTO>()
                .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl))
                .ForMember(d => d.ImagePublicId, o => o.MapFrom(s => s.ImagePublicId));
            
            // ===================== CHATBOT =====================
            CreateMap<ChatbotConversation, ChatbotConversationsResponseDTO>().ReverseMap();
            CreateMap<ChatbotMessage, ChatbotMessagesResponseDTO>().ReverseMap();
            CreateMap<ChatbotConversationUpdateDTO, ChatbotConversation>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ChatbotMessage, ChatbotMessageCreateDTO>().ReverseMap();
        }
    }
}
