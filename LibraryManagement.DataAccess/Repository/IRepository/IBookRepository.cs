using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.DataAccess.IRepository;

public interface IBookRepository : IBaseRepository<Book>
{
    Task<(List<BookDetailsViewModel> Books, int TotalRecords)> GetPaginatedListAsync(PaginationFilter paginationFilter, List<FilterCriteria> lstFilters);
}
