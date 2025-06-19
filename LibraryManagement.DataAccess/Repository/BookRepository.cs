using System.Linq.Expressions;
using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DataAccess.Repository;

public class BookRepository : BaseRepository<Book>, IBookRepository
{
    private readonly ApplicationDbContext _context;

    public BookRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(List<BookDetailsViewModel> Books, int TotalRecords)> GetPaginatedListAsync(PaginationFilter paginationFilter, List<FilterCriteria> lstFilters)
    {
        IQueryable<Book> baseQuery = _context.Books;

        baseQuery = FilterParser.ApplyFilters(baseQuery, lstFilters);

        if (!string.IsNullOrWhiteSpace(paginationFilter.SearchTerm))
        {
            baseQuery = baseQuery.Where(b =>
                b.BookName.ToLower().Contains(paginationFilter.SearchTerm.ToLower()) ||
                b.AutherName.ToLower().Contains(paginationFilter.SearchTerm.ToLower())
            );
        }

        int nTotalRecords = await baseQuery.CountAsync();

        string[] parts = paginationFilter.SortField!.Split('.');
        var parameter = Expression.Parameter(typeof(Book), "x");
        Expression sortProperty = parts.Aggregate((Expression)parameter, Expression.PropertyOrField);
        var sortExpression = Expression.Lambda<Func<Book, object>>(Expression.Convert(sortProperty, typeof(object)), parameter);

        if (paginationFilter.IsAscending)
        {
            baseQuery = baseQuery.OrderBy(sortExpression);
        }
        else
        {
            baseQuery = baseQuery.OrderByDescending(sortExpression);
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
                PublisherId = b.PublisherId,
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
}
