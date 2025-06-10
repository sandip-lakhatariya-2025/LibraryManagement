using System.Dynamic;
using System.Net;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services.IServices;
using LibraryManagement.Utility;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooks(PaginationFilter paginationFilter, IQueryCollection queryParams, string? fields)
    {
        var (books, totalRecords) = await _bookRepository.GetPaginatedListAsync(paginationFilter, queryParams);
        int totalPages = (int)Math.Ceiling((double)totalRecords / paginationFilter.PageSize);

        var shapedBooks = books.Select(book => ObjectShaper.GetShapedObject(book, fields)).ToList();

        return CommonHelper.CreatePagedResponse(
            data: shapedBooks,
            nPageNumber: paginationFilter.PageNumber,
            nPageSize: paginationFilter.PageSize,
            ntotalRecords: totalRecords,
            ntotalPages: totalPages,
            statusCode: HttpStatusCode.OK,
            bIsSuccess: true,
            sMessage: "Books retrieved successfully"
        );
    }

    public async Task<BooksViewModel?> GetBookById(long id)
    {
        return await _bookRepository.GetFirstOrDefaultSelectedAsync(id);
    }

    public async Task<Response<BooksViewModel?>> AddBook(BooksViewModel booksViewModel)
    {
        Book newBook = new Book
        {
            BookName = booksViewModel.BookName,
            AutherName = booksViewModel.AutherName,
            Description = booksViewModel.Description,
            Price = booksViewModel.Price,
            TotalCopies = booksViewModel.TotalCopies,
            PublisherId = booksViewModel.PublisherId
        };

        bool isSuccess = await _bookRepository.InsertAsync(newBook);

        if (isSuccess)
        {
            BooksViewModel? addedBook = await GetBookById(newBook.Id);
            return CommonHelper.CreateResponse(addedBook, HttpStatusCode.OK, true, "Book added successfully.");
        }

        return CommonHelper.CreateResponse<BooksViewModel?>(null, HttpStatusCode.BadRequest, false, $"Failed to add book.");
    }

    public async Task<Response<BooksViewModel?>> UpdateBook(BooksViewModel booksViewModel)
    {
        Book? existingBook = await _bookRepository.GetFirstOrDefaultAsync(booksViewModel.Id);

        if (existingBook != null)
        {
            existingBook.BookName = booksViewModel.BookName;
            existingBook.AutherName = booksViewModel.AutherName;
            existingBook.Description = booksViewModel.Description;
            existingBook.Price = booksViewModel.Price;
            existingBook.TotalCopies = booksViewModel.TotalCopies;

            bool isSuccess = await _bookRepository.UpdateAsync(existingBook);

            if (isSuccess)
            {
                BooksViewModel? updatedBook = await GetBookById(existingBook.Id);
                return CommonHelper.CreateResponse(updatedBook, HttpStatusCode.OK, true, "Book Updated successfully");
            }
        }

        return CommonHelper.CreateResponse<BooksViewModel?>(null, HttpStatusCode.BadRequest, false, "Book not found.");
    }

    public async Task<Response<List<BooksViewModel>>> UpdateListOfBooks(List<BooksViewModel> booksViewModels)
    {
        List<Book> books = new List<Book>();

        foreach (var bookViewModel in booksViewModels)
        {
            Book? existingBook = await _bookRepository.GetFirstOrDefaultAsync(bookViewModel.Id);

            if (existingBook != null)
            {
                existingBook.BookName = bookViewModel.BookName;
                existingBook.AutherName = bookViewModel.AutherName;
                existingBook.Description = bookViewModel.Description;
                existingBook.Price = bookViewModel.Price;
                existingBook.TotalCopies = bookViewModel.TotalCopies;

                books.Add(existingBook);
            }
            else
            {
                return CommonHelper.CreateResponse(new List<BooksViewModel>(), HttpStatusCode.BadRequest, false, "Some books are not found.");
            }
        }

        bool isSuccess = await _bookRepository.UpdateRangeAsync(books);

        if (isSuccess)
        {
            List<BooksViewModel> updatedBooks = await _bookRepository.GetBooksDtoByIds(booksViewModels.Select(b => b.Id).ToList());
            return CommonHelper.CreateResponse(updatedBooks, HttpStatusCode.OK, true, "All selected Books has been updated successfully");
        }

        return CommonHelper.CreateResponse(new List<BooksViewModel>(), HttpStatusCode.BadRequest, false, "Some error occured while updating the books.");
    }

    public async Task<(bool isSuccess, string message)> DeleteBook(long id)
    {
        Book? existingBook = await _bookRepository.GetFirstOrDefaultAsync(id);

        if (existingBook != null)
        {
            bool isSuccess = await _bookRepository.DeleteAsync(existingBook);
            if (isSuccess)
            {
                return (true, "Book Deleted successfully");
            }
        }

        return (false, "Book not found.");
    }

    public async Task<(bool isSuccess, string message)> DeleteListOfBooks(List<int> ids)
    {
        List<Book> books = new List<Book>();

        foreach (var id in ids)
        {
            Book? existingBook = await _bookRepository.GetFirstOrDefaultAsync(id);
            if (existingBook != null)
            {
                books.Add(existingBook);
            }
            else
            {
                return (false, "Some books are not found.");
            }
        }

        bool isSuccess = await _bookRepository.DeleteRangeAsync(books);
        
        if (isSuccess)
        {
            return (true, "All selected Books has been deleted successfully");
        }

        return (false, "Some error occured while deleting the books.");
    }

}
