using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Dtos.RequestDtos;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DataAccess.Repository;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    internal DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context) 
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expFilter)
    {
        return await _dbSet.Where(expFilter).FirstOrDefaultAsync();
    }

    public async Task<TResult?> GetFirstOrDefaultAsync<TResult>(Expression<Func<T, bool>> expFilter, Expression<Func<T, TResult>> expSelector)
    {
        return await _dbSet.Where(expFilter).Select(expSelector).FirstOrDefaultAsync();
    }

    public async Task<TResult?> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> filter,
        IConfigurationProvider mapperConfig)
    {
        return await _dbSet
            .Where(filter)
            .ProjectTo<TResult>(mapperConfig)
            .FirstOrDefaultAsync();
    }

    public Task<List<TResult>> GetPaginatedListAsync<TResult>(
        PaginationFilter objPaginationFilter, 
        Expression<Func<T, bool>> expFilter, 
        Expression<Func<T, TResult>> expSelector)
    {
        throw new NotImplementedException();
    }

    public async Task<List<TResult>> GetListAsync<TResult>(
        Expression<Func<T, bool>> expFilter,
        IConfigurationProvider mapperConfig)
    {
        return await _dbSet.Where(expFilter).ProjectTo<TResult>(mapperConfig).ToListAsync();
    }

        public async Task<List<TResult>> GetListAsync<TResult>(
        Expression<Func<T, bool>> expFilter,
        Expression<Func<T, TResult>> expSelector)
    {
        return await _dbSet.Where(expFilter).Select(expSelector).ToListAsync();
    }

    public async Task InsertAsync(T objEntity)
    {
        await _context.AddAsync(objEntity);
    }

    public async Task InsertListAsync(List<T> lstEntity)
    {
        await _context.AddRangeAsync(lstEntity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void UpdateEntity(T objEntity)
    {
        _context.Update(objEntity);
    }

    public void UpdateList(List<T> lstEntity)
    {
        _context.UpdateRange(lstEntity);
    }

    public void DeleteEntity(T objEntity) 
    {
        _context.Remove(objEntity);
    }

    public void DeleteList(List<T> lstEntity)
    {
        _context.RemoveRange(lstEntity);
    }

    public async Task<bool> ExistAsync(Expression<Func<T, bool>> expFilter)
    {
        return await _dbSet.AnyAsync(expFilter);
    }
}
