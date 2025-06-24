using AutoMapper;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;

namespace LibraryManagement.Utility.MappingProfiles;

public class UserMappingProfile : Profile
{
    public UserMappingProfile() 
    {
        CreateMap<RegisterDto, User>();
        CreateMap<LoginDto, User>();
        CreateMap<User, RegistrationResultDto>();        
    }
}
