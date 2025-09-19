using AutoMapper;
using BLL.DTO.Address;
using BLL.DTO.FarmProfile;
using BLL.DTO.SupportedBanks;
// using BLL.DTO.SustainabilityCertifications;
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
        
        CreateMap<UserCreateDTO, User>().ReverseMap();
        CreateMap<UserResponseDTO, User>().ReverseMap();
        CreateMap<UserUpdateDTO, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

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

    }
}