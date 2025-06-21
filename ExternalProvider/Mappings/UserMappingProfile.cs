using AutoMapper;
using ExternalProvider.Models.Domain;
using ExternalProvider.Models.Dto;

namespace ExternalProvider.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.First_Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Last_Name));  
        }
    }
}
