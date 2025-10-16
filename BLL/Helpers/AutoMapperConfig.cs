using AutoMapper;
using BLL.DTO.Address;
using BLL.DTO.Cart;
using BLL.DTO.CO2;
using BLL.DTO.FarmProfile;
using BLL.DTO.MediaLink;
using BLL.DTO.Order;
using BLL.DTO.Product;
using BLL.DTO.ProductCategory;
using BLL.DTO.ProductCertificate;
using BLL.DTO.ProductRegistration;
using BLL.DTO.User;
using DAL.Data.Models;

namespace BLL.Helpers;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        //Khi có 2 trường khác tên, ví dụ: studentName và Name
        //CreateMap<StudentDTO, Student>().ForMember(n => n.studentName, opt => opt.MapFrom(x => x.Name)).ReverseMap();

        //Khi muốn map tất cả ngoại trừ studentName
        //CreateMap<StudentDTO, Student>().ReverseMap().ForMember(n => n.studentName, opt => opt.Ignore());

        //Khi giá trị bị null
        //CreateMap<StudentDTO, Student>().ReverseMap()
        //.ForMember(n => n.Address, opt => opt.MapFrom(n => string.IsNullOrEmpty(n.Address) ? "No value found" : n.Address));

        // User mappings
        CreateMap<UserCreateDTO, User>().ReverseMap();
        CreateMap<StaffCreateDTO, User>().ReverseMap();
        CreateMap<UserUpdateDTO, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<UserResponseDTO, User>().ReverseMap()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.UserAddresses));
        CreateMap<UserAddressCreateDTO, Address>().ReverseMap();
        CreateMap<UserAddressUpdateDTO, Address>().ReverseMap();
        CreateMap<UserAddressUpdateDTO, UserAddress>().ReverseMap();

        // FarmProfile mappings
        CreateMap<FarmProfileCreateDto, FarmProfile>().ReverseMap();
        CreateMap<FarmProfile, FarmProfileResponseDTO>().ReverseMap();
        CreateMap<FarmProfileUpdateDTO, FarmProfile>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Address mappings
        CreateMap<Address, AddressResponseDTO>().ReverseMap();
        CreateMap<UserAddress, AddressResponseDTO>()
            .IncludeMembers(src => src.Address)
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));
        CreateMap<FarmProfileCreateDto, Address>().ReverseMap();
        CreateMap<FarmProfileUpdateDTO, Address>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // CO2Footprint mappings
        CreateMap<Fertilizer, CO2FootprintCreateDTO>().ReverseMap();
        CreateMap<EnvironmentalDatum, CO2FootprintCreateDTO>().ReverseMap();
        CreateMap<EnergyUsage, CO2FootprintCreateDTO>().ReverseMap();
        CreateMap<EnvironmentalDatum, CO2FootprintResponseDTO>()
            .ForMember(dest => dest.EnergyUsage, opt => opt.MapFrom(src => src.EnergyUsage))
            .ForMember(dest => dest.Fertilizer, opt => opt.MapFrom(src => src.Fertilizer));
        CreateMap<EnergyUsageDTO, EnergyUsage>().ReverseMap();
        CreateMap<FertilizerDTO, Fertilizer>().ReverseMap();

        // ProductCategory mappings
        CreateMap<ProductCategoryCreateDTO, ProductCategory>().ReverseMap();
        CreateMap<ProductCategoryUpdateDTO, ProductCategory>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ProductCategory, ProductCategoryResponseDTO>().ReverseMap();
        //medialink map 
        CreateMap<MediaLink, MediaLinkItemDTO>()
          .ForMember(d => d.ImagePublicId, o => o.MapFrom(s => s.ImagePublicId))
          .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl))
          .ForMember(d => d.SortOrder, o => o.MapFrom(s => s.SortOrder))
          .ForMember(d => d.Purpose, o => o.MapFrom(s => s.Purpose.ToString().ToLowerInvariant()))
          .ForMember(d => d.Id, o => o.MapFrom(s => s.Id));

        //// Product mappings
        //CreateMap<BLL.DTO.Product.ProductCreateDTO, Product>().ReverseMap();
        //CreateMap<BLL.DTO.Product.ProductUpdateDTO, Product>()
        //    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        //CreateMap<Product, BLL.DTO.Product.ProductResponseDTO>().ReverseMap();

        // Cart mappings
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
        CreateMap<MediaLink, ImagesDTO>().ReverseMap();

        // Mapping cho ProductRegistration
        CreateMap<ProductRegistration, ProductRegistrationReponseDTO>()
            .ForMember(d => d.ManualUrl, opt => opt.MapFrom(s => s.ManualUrls))
            .ForMember(d => d.ManualPublicUrl, opt => opt.MapFrom(s => s.PublicUrl))
            .ForMember(d => d.EnergyEfficiencyRating, opt => opt.MapFrom(s => s.EnergyEfficiencyRating.HasValue ? s.EnergyEfficiencyRating.Value.ToString() : null))
            .ReverseMap();

        CreateMap<ProductRegistration, ProductRegistrationReponseDTO>()
            .ForMember(d => d.ManualUrl, o => o.MapFrom(s => s.ManualUrls))        
            .ForMember(d => d.ManualPublicUrl, o => o.MapFrom(s => s.PublicUrl)); 

        CreateMap<ProductRegistration, ProductRegistrationReponseDTO>()
            .ForMember(d => d.ProductImages, o => o.Ignore())
            .ForMember(d => d.CertificateFiles, o => o.Ignore());
        CreateMap<ProductRegistrationCreateDTO, ProductRegistration>()
            // BỎ QUA rating để service tự parse string -> int?
            .ForMember(d => d.EnergyEfficiencyRating, o => o.Ignore())
            // Giữ mapping Dimensions như bạn đang có
            .ForMember(dest => dest.DimensionsCm, opt => opt.MapFrom(src => new Dictionary<string, decimal>
            {
        { "Width",  src.DimensionsCm.Width },
        { "Height", src.DimensionsCm.Height },
        { "Length", src.DimensionsCm.Length }
            }));

        CreateMap<ProductRegistrationUpdateDTO, ProductRegistration>()
            // BỎ QUA rating để service tự parse string -> int?
            .ForMember(d => d.EnergyEfficiencyRating, o => o.Ignore())
            .ForMember(dest => dest.DimensionsCm, opt => opt.MapFrom(src => new Dictionary<string, decimal>
            {
        { "Width",  src.DimensionsCm.Width },
        { "Height", src.DimensionsCm.Height },
        { "Length", src.DimensionsCm.Length }
            }))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ProductRegistration, Product>()
            .ForMember(d => d.Id, o => o.Ignore()) // tránh insert id nếu DB tự sinh
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
            // Nếu Product có các trường như Slug/PublicUrl/CreatedAt/UpdatedAt do service set:
            .ForMember(d => d.Slug, o => o.Ignore())
            .ForMember(d => d.PublicUrl, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        // Order mappings
        CreateMap<OrderPreviewCreateDTO, OrderPreviewResponseDTO>()
            .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());
        // Certificate mapping 
        CreateMap<ProductCertificateCreateDTO, ProductCertificate>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

        CreateMap<ProductCertificateUpdateDTO, ProductCertificate>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());
        CreateMap<ProductCertificate, ProductCertificateResponseDTO>();


        //CreateMap<ProductResponseDTO, Product>().ReverseMap();
        CreateMap<OrderPreviewResponseDTO, DAL.Data.Models.Order>()
            .ForMember(dest => dest.OrderDetails, opt => opt.Ignore())
            .ForMember(dest => dest.Address, opt => opt.Ignore());
        CreateMap<DAL.Data.Models.Order, OrderResponseDTO>();
        CreateMap<OrderDetail, OrderDetailsResponseDTO>()
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));
        CreateMap<MediaLink, ProductImageResponseDTO>().ReverseMap();

        // ===================== Product mappings =====================

        // Nếu bạn có ProductCreateDTO/UpdateDTO riêng cho Product:
        CreateMap<BLL.DTO.Product.ProductCreateDTO, Product>().ReverseMap();

        CreateMap<BLL.DTO.Product.ProductUpdateDTO, Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Product -> ProductResponseDTO (ép EnergyEfficiencyRating int? -> string)
        CreateMap<Product, BLL.DTO.Product.ProductResponseDTO>()
            .ForMember(d => d.EnergyEfficiencyRating,
                o => o.MapFrom(s => s.EnergyEfficiencyRating.HasValue
                                    ? s.EnergyEfficiencyRating.Value.ToString()
                                    : null));

        // Bản rút gọn cho list
        CreateMap<Product, BLL.DTO.Product.ProductListItemDTO>()
            .ForMember(d => d.EnergyEfficiencyRating,
                o => o.MapFrom(s => s.EnergyEfficiencyRating.HasValue
                                    ? s.EnergyEfficiencyRating.Value.ToString()
                                    : null));


        CreateMap<ProductRegistration, ProductRegistrationReponseDTO>()
    // enum -> string
    .ForMember(d => d.Status,
        o => o.MapFrom(s => s.Status.ToString()))
    // int? -> string
    .ForMember(d => d.EnergyEfficiencyRating,
        o => o.MapFrom(s => s.EnergyEfficiencyRating.HasValue ? s.EnergyEfficiencyRating.Value.ToString() : null))
    // Dictionary<string, decimal> -> Dictionary<string, object>
    .ForMember(d => d.DimensionsCm,
        o => o.MapFrom(s => s.DimensionsCm != null
            ? s.DimensionsCm.ToDictionary(k => k.Key, v => (object)v.Value)
            : new Dictionary<string, object>()))
    // giữ nguyên specs, fallback rỗng nếu null
    .ForMember(d => d.Specifications,
        o => o.MapFrom(s => s.Specifications ?? new Dictionary<string, object>()))
    // ảnh sẽ được hydrate riêng trong service
    .ForMember(d => d.ProductImages, o => o.Ignore());
    }


}
