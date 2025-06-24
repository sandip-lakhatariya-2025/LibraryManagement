using AutoMapper;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;

namespace LibraryManagement.Utility.MappingProfiles;

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<CustomerCreateDto, Customer>();
        CreateMap<Customer, CustomerResultDto>();
    }
}
