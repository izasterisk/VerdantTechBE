using AutoMapper;
using BLL.DTO.Address;
using BLL.DTO.Cart;
using BLL.DTO.CO2;
using BLL.DTO.FarmProfile;
using BLL.DTO.ProductCategory;
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
            .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.UserAddresses.Select(ua => ua.Address)));
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
        
        // Product mappings
        CreateMap<BLL.DTO.Product.ProductCreateDTO, Product>().ReverseMap();
        CreateMap<BLL.DTO.Product.ProductUpdateDTO, Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Product, BLL.DTO.Product.ProductResponseDTO>().ReverseMap();

        // Cart mappings
        CreateMap<CartAddDTO, CartItem>().ReverseMap();
        CreateMap<Cart, CartResponseDTO>()
            .ForMember(d => d.UserInfoDTO, o => o.MapFrom(s => s.Customer))
            .ForMember(d => d.CartItems,  o => o.MapFrom(s => s.CartItems));
        CreateMap<CartItem, CartItemDTO>()
            .ForMember(d => d.ProductId,     o => o.MapFrom(s => s.ProductId))
            .ForMember(d => d.Quantity,      o => o.MapFrom(s => s.Quantity))
            .ForMember(d => d.ProductName,   o => o.MapFrom(s => s.Product.ProductName))
            .ForMember(d => d.Slug,          o => o.MapFrom(s => s.Product.Slug))
            .ForMember(d => d.Description,   o => o.MapFrom(s => s.Product.Description))
            .ForMember(d => d.UnitPrice,     o => o.MapFrom(s => s.Product.UnitPrice))
            .ForMember(d => d.Images,        o => o.MapFrom(s => s.Product.Images))
            .ForMember(d => d.IsActive,      o => o.MapFrom(s => s.Product.IsActive))
            .ForMember(d => d.SoldCount,     o => o.MapFrom(s => s.Product.SoldCount))
            .ForMember(d => d.RatingAverage, o => o.MapFrom(s => s.Product.RatingAverage));

    }
}