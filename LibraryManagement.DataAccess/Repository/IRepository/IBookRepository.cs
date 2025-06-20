using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.DataAccess.IRepository;

public interface IBookRepository : IBaseRepository<Book>
{
    Task<(List<BookDetailsViewModel> Books, int TotalRecords)> GetPaginatedListAsyncV1(PaginationFilter paginationFilter, List<FilterCriteria> lstFilters);
    Task<(List<BookDetailsViewModel> Books, int TotalRecords)> GetPaginatedListAsyncV2(PaginationFilter paginationFilter, List<List<FilterCriteria>> lstFilters);
}
