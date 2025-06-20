using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;
using LibraryManagement.Utility;

namespace LibraryManagement.DataAccess.IRepository;

public interface IBookRepository : IBaseRepository<Book>
{
    Task<(List<BookDetailsDto> Books, int TotalRecords)> GetPaginatedListAsyncV1(PaginationFilter paginationFilter, List<FilterCriteria> lstFilters);
    Task<(List<BookDetailsDto> Books, int TotalRecords)> GetPaginatedListAsyncV2(PaginationFilter paginationFilter, List<List<FilterCriteria>> lstFilters);
}
