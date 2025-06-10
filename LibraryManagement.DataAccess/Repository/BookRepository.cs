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

    public async Task<(List<BooksViewModel> Books, int TotalRecords)> GetPaginatedListAsync(PaginationFilter paginationFilter, IQueryCollection queryParams)
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

        int totalRecords = await baseQuery.CountAsync();

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

        List<BooksViewModel> books = await baseQuery
            .Select(b => new BooksViewModel
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

        return (books, totalRecords);
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

    public async Task<List<BooksViewModel>> GetBooksDtoByIds(List<long> ids)
    {
        return await _context.Books
            .Where(b => ids.Contains(b.Id))
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

    public async Task<bool> InsertAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> InsertRangeAsync(List<Book> books)
    {
        await _context.Books.AddRangeAsync(books);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateRangeAsync(List<Book> books)
    {
        _context.Books.UpdateRange(books);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Book book)
    {
        _context.Books.Remove(book);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteRangeAsync(List<Book> books)
    {
        _context.Books.RemoveRange(books);
        return await _context.SaveChangesAsync() > 0;
    }
}
