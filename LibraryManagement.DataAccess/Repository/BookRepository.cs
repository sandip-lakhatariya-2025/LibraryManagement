using System.Linq.Expressions;
using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DataAccess.Repository;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BooksViewModel>> GetAllAsync()
    {
        return await _context.Books.Select(b => new BooksViewModel
        {
            Id = b.Id,
            BookName = b.BookName,
            AutherName = b.AutherName,
            Description = b.Description,
            Price = b.Price,
            TotalCopies = b.TotalCopies
        }).OrderBy(b => b.Id).ToListAsync();
    }

    public async Task<(List<BookDetailsViewModel> Books, int TotalRecords)> GetPaginatedListAsync(PaginationFilter paginationFilter, IQueryCollection queryParams)
    {
        IQueryable<Book> baseQuery = _context.Books;

        var filters = FilterParser.ParseFilters(queryParams);
        baseQuery = FilterParser.ApplyFilters(baseQuery, filters);

        if (!string.IsNullOrWhiteSpace(paginationFilter.SearchTerm))
        {
            baseQuery = baseQuery.Where(b =>
                b.BookName.ToLower().Contains(paginationFilter.SearchTerm.ToLower()) ||
                b.AutherName.ToLower().Contains(paginationFilter.SearchTerm.ToLower())
            );
        }

        int nTotalRecords = await baseQuery.CountAsync();

        Expression<Func<Book, Object>> keySelector = paginationFilter.SortField!.ToLower() switch
        {
            "bookname" => b => b.BookName,
            "authername" => b => b.AutherName,
            "price" => b => b.Price,
            "tatalcopies" => b => b.TotalCopies,
            _ => b => b.Id
        };

        if (paginationFilter.IsAscending)
        {
            baseQuery = baseQuery.OrderBy(keySelector);
        }
        else
        {
            baseQuery = baseQuery.OrderByDescending(keySelector);
        }

        int skipCount = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

        List<BookDetailsViewModel> lstBooks = await baseQuery
            .Select(b => new BookDetailsViewModel
            {
                Id = b.Id,
                BookName = b.BookName,
                AutherName = b.AutherName,
                Description = b.Description,
                Price = b.Price,
                TotalCopies = b.TotalCopies,
                PublisherDetails = new PublisherViewModel
                {
                    Id = b.Publisher.Id,
                    Name = b.Publisher.Name,
                    Address = b.Publisher.Address
                },
                ReaderDetails = new ReaderDetailsViewModel
                {
                    CurrentReaders = b.CustomerBooks.Select(cb => new CustomerViewModel
                    {
                        Id = cb.Customer.Id,
                        Name = cb.Customer.Name,
                        Email = cb.Customer.Email
                    }).ToList()
                }
            })
            .Skip(skipCount)
            .Take(paginationFilter.PageSize)
            .ToListAsync();

        return (lstBooks, nTotalRecords);
    }


    public async Task<BooksViewModel?> GetFirstOrDefaultSelectedAsync(long id)
    {
        return await _context.Books.Where(b => b.Id == id).Select(b => new BooksViewModel
        {
            Id = b.Id,
            BookName = b.BookName,
            AutherName = b.AutherName,
            Description = b.Description,
            Price = b.Price,
            TotalCopies = b.TotalCopies
        }).FirstOrDefaultAsync();
    }

    public async Task<Book?> GetFirstOrDefaultAsync(long id)
    {
        return await _context.Books.Where(b => b.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<BooksViewModel>> GetBooksDtoByIds(List<long> lstIds)
    {
        return await _context.Books
            .Where(b => lstIds.Contains(b.Id))
            .Select(b => new BooksViewModel
            {
                Id = b.Id,
                BookName = b.BookName,
                AutherName = b.AutherName,
                Description = b.Description,
                Price = b.Price,
                TotalCopies = b.TotalCopies
            })
            .ToListAsync();
    }

    public async Task<bool> InsertAsync(Book objBook)
    {
        await _context.Books.AddAsync(objBook);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> InsertRangeAsync(List<Book> lstBooks)
    {
        await _context.Books.AddRangeAsync(lstBooks);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Book objBook)
    {
        _context.Books.Update(objBook);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateRangeAsync(List<Book> lstBooks)
    {
        _context.Books.UpdateRange(lstBooks);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Book objBook)
    {
        _context.Books.Remove(objBook);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteRangeAsync(List<Book> lstBooks)
    {
        _context.Books.RemoveRange(lstBooks);
        return await _context.SaveChangesAsync() > 0;
    }
}
