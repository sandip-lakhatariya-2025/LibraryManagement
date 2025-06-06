using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.DataAccess.IRepository;

public interface IBookRepository
{
    Task<List<BooksViewModel>> GetAllAsync();
    Task<(List<BooksViewModel> Books, int TotalRecords)> GetPaginatedListAsync(PaginationFilter paginationFilter, IQueryCollection queryParams);
    Task<BooksViewModel?> GetFirstOrDefaultSelectedAsync(long id);
    Task<Book?> GetFirstOrDefaultAsync(long id);
    Task<List<BooksViewModel>> GetBooksDtoByIds(List<long> ids);
    Task<bool> InsertAsync(Book book);
    Task<bool> InsertRangeAsync(List<Book> books);
    Task<bool> UpdateAsync(Book book);
    Task<bool> UpdateRangeAsync(List<Book> books);
    Task<bool> DeleteAsync(Book book);
    Task<bool> DeleteRangeAsync(List<Book> books);
}
