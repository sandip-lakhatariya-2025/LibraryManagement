using AutoMapper;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;

namespace LibraryManagement.Common.MappingProfiles;

public class BookMappingProfile : Profile
{
    public BookMappingProfile() {
        CreateMap<BookCreateDto, Book>();
        CreateMap<Book, BookUpdateDto>();
        CreateMap<Book, BookResultDto>();
    }
}
