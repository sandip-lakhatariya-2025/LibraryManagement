using System.Linq.Expressions;
using LibraryManagement.Models.Dtos.RequestDtos;

namespace LibraryManagement.DataAccess.Repository.IRepository;

public interface IBaseRepository<T> where T : class
{

    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);

    Task<TResult?> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> expFilter, 
        Expression<Func<T, TResult>> expSelector
    );

    Task<List<TResult>> GetPaginatedListAsync<TResult>(
        PaginationFilter objPaginationFilter,
        Expression<Func<T, bool>> expFilter,
        Expression<Func<T, TResult>> expSelector
    );

    Task<List<TResult>> GetListAsync<TResult>(
        Expression<Func<T, bool>> expFilter,
        Expression<Func<T, TResult>> expSelector
    );

    Task InsertAsync(T objEntity);

    Task InsertListAsync(List<T> lstEntity);

    void UpdateEntity(T objEntity);

    void UpdateList(List<T> lstEntity);

    void DeleteEntity(T objEntity);

    void DeleteList(List<T> lstEntity);

    Task<bool> ExistAsync(Expression<Func<T, bool>> expFilter);

    Task SaveChangesAsync();
}
