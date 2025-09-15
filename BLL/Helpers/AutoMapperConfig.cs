using AutoMapper;
using BLL.DTO.FarmProfile;
using BLL.DTO.SupportedBanks;
using BLL.DTO.SustainabilityCertifications;
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
        CreateMap<UserReadOnlyDTO, User>().ReverseMap();
        CreateMap<UserUpdateDTO, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<SustainabilityCertificationsCreateDTO, SustainabilityCertification>().ReverseMap();
        CreateMap<SustainabilityCertificationsReadOnlyDTO, SustainabilityCertification>().ReverseMap();
        CreateMap<SustainabilityCertificationsUpdateDTO, SustainabilityCertification>().ReverseMap()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<SupportedBanksCreateDTO, SupportedBank>().ReverseMap();
        CreateMap<SupportedBanksReadOnlyDTO, SupportedBank>().ReverseMap();
        CreateMap<SupportedBanksUpdateDTO, SupportedBank>().ReverseMap()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<FarmProfile, FarmProfileCreateDto>();
        CreateMap<FarmProfileCreateDto, FarmProfile>()
            .ForMember(d => d.UserId,    o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());
    }
}