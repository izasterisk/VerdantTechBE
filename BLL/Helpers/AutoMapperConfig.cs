using AutoMapper;
using BLL.DTO.Address;
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
        
        // ProductCategory mappings
        CreateMap<ProductCategoryCreateDTO, ProductCategory>().ReverseMap();
        CreateMap<ProductCategoryUpdateDTO, ProductCategory>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ProductCategory, ProductCategoryResponseDTO>().ReverseMap();
    }
}