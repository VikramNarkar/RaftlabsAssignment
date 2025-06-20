using AutoMapper;
using ExternalProvider.Models.Domain;
using ExternalProvider.Models.Dto;

namespace ExternalProvider.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserDto, User>();
        }
    }
}
