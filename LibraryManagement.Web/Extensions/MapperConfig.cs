using AutoMapper;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;

namespace LibraryManagement.Web.Extensions;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<BookCreateDto, Book>();
        CreateMap<BookUpdateDto, Book>();
        CreateMap<Book, BookResultDto>();

        CreateMap<CustomerCreateDto, Customer>();
        CreateMap<Customer, CustomerResultDto>();

        CreateMap<RegisterDto, User>();
        CreateMap<LoginDto, User>();
        CreateMap<User, RegistrationResultDto>();  
    }
}
