using System.Dynamic;
using LibraryManagement.Common;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Services.IServices;

public interface IBookService
{
    Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooks(PaginationFilter paginationFilter, IQueryCollection queryParams, string? sFields);
    Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooks(PaginationFilter paginationFilter, string? sFilters, string? sFields);
    Task<BooksViewModel?> GetBookById(long id);
    Task<Response<ExpandoObject>> GetBookByIdV2(long id, string? sFields);
    Task<Response<BooksViewModel?>> AddBook(BooksViewModel objBookViewModel);
    Task<Response<BooksViewModel?>> UpdateBook(BooksViewModel objBookViewModel);
    Task<Response<List<BooksViewModel>>> UpdateListOfBooks(List<BooksViewModel> lstBooksViewModels);
    Task<Response<bool>> DeleteBook(long id);
    Task<Response<bool>> DeleteListOfBooks(List<int> lstIds);
}
