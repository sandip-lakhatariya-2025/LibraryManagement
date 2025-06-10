using System.Dynamic;
using LibraryManagement.Common;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Services.IServices;

public interface IBookService
{
    Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooks(PaginationFilter paginationFilter, IQueryCollection queryParams, string? fields);
    Task<BooksViewModel?> GetBookById(long id);
    Task<Response<BooksViewModel?>> AddBook(BooksViewModel booksViewModel);
    Task<Response<BooksViewModel?>> UpdateBook(BooksViewModel booksViewModel);
    Task<Response<List<BooksViewModel>>> UpdateListOfBooks(List<BooksViewModel> booksViewModels);
    Task<(bool isSuccess, string message)> DeleteBook(long id);
    Task<(bool isSuccess, string message)> DeleteListOfBooks(List<int> ids);
}
