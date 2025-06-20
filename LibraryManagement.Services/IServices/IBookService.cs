using System.Dynamic;
using LibraryManagement.Common;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Services.IServices;

public interface IBookService
{
    Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooksV1(PaginationFilter paginationFilter, IQueryCollection queryParams, string? sFields);
    Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooksV2(PaginationFilter paginationFilter, string? sFilters, string? sFields);
    Task<BookResultDto?> GetBookById(long id);
    Task<Response<ExpandoObject>> GetBookByIdV2(long id, string? sFields);
    Task<Response<BookResultDto?>> AddBook(BookCreateDto objBookCreateDto);
    Task<Response<BookResultDto?>> UpdateBook(BookUpdateDto objBookUpdateDto);
    Task<Response<List<BookResultDto>>> UpdateListOfBooks(List<BookUpdateDto> lstBookUpdateDtos);
    Task<Response<bool>> DeleteBook(long id);
    Task<Response<bool>> DeleteListOfBooks(List<int> lstIds);
}
