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

    public async Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooks(PaginationFilter paginationFilter, IQueryCollection queryParams, string? sFields)
    {
        var (lstBooks, nTotalRecords) = await _bookRepository.GetPaginatedListAsync(paginationFilter, queryParams);
        int nTotalPages = (int)Math.Ceiling((double)nTotalRecords / paginationFilter.PageSize);

        var shapedBooks = lstBooks.Select(book => ObjectShaper.GetShapedObject(book, sFields)).ToList();

        return CommonHelper.CreatePagedResponse(
            data: shapedBooks,
            nPageNumber: paginationFilter.PageNumber,
            nPageSize: paginationFilter.PageSize,
            ntotalRecords: nTotalRecords,
            ntotalPages: nTotalPages,
            statusCode: HttpStatusCode.OK,
            bIsSuccess: true,
            sMessage: "Books retrieved successfully"
        );
    }

    public async Task<BooksViewModel?> GetBookById(long id)
    {
        return await _bookRepository.GetFirstOrDefaultSelectedAsync(id);
    }

    public async Task<Response<BooksViewModel?>> AddBook(BooksViewModel objBookViewModel)
    {
        Book objNewBook = new Book
        {
            BookName = objBookViewModel.BookName,
            AutherName = objBookViewModel.AutherName,
            Description = objBookViewModel.Description,
            Price = objBookViewModel.Price,
            TotalCopies = objBookViewModel.TotalCopies,
            PublisherId = objBookViewModel.PublisherId
        };

        bool bIsSuccess = await _bookRepository.InsertAsync(objNewBook);

        if (bIsSuccess)
        {
            BooksViewModel? addedBook = await GetBookById(objNewBook.Id);
            return CommonHelper.CreateResponse(addedBook, HttpStatusCode.OK, true, "Book added successfully.");
        }

        return CommonHelper.CreateResponse<BooksViewModel?>(null, HttpStatusCode.BadRequest, false, $"Failed to add book.");
    }

    public async Task<Response<BooksViewModel?>> UpdateBook(BooksViewModel objBookViewModel)
    {
        Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(objBookViewModel.Id);

        if (objExistingBook != null)
        {
            objExistingBook.BookName = objBookViewModel.BookName;
            objExistingBook.AutherName = objBookViewModel.AutherName;
            objExistingBook.Description = objBookViewModel.Description;
            objExistingBook.Price = objBookViewModel.Price;
            objExistingBook.TotalCopies = objBookViewModel.TotalCopies;

            bool bIsSuccess = await _bookRepository.UpdateAsync(objExistingBook);

            if (bIsSuccess)
            {
                BooksViewModel? updatedBook = await GetBookById(objExistingBook.Id);
                return CommonHelper.CreateResponse(updatedBook, HttpStatusCode.OK, true, "Book Updated successfully");
            }
        }

        return CommonHelper.CreateResponse<BooksViewModel?>(null, HttpStatusCode.NotFound, false, "Book not found.");
    }

    public async Task<Response<List<BooksViewModel>>> UpdateListOfBooks(List<BooksViewModel> listBooksViewModels)
    {
        List<Book> listBooks = new List<Book>();

        foreach (var objBookViewModel in listBooksViewModels)
        {
            Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(objBookViewModel.Id);

            if (objExistingBook != null)
            {
                objExistingBook.BookName = objBookViewModel.BookName;
                objExistingBook.AutherName = objBookViewModel.AutherName;
                objExistingBook.Description = objBookViewModel.Description;
                objExistingBook.Price = objBookViewModel.Price;
                objExistingBook.TotalCopies = objBookViewModel.TotalCopies;

                listBooks.Add(objExistingBook);
            }
            else
            {
                return CommonHelper.CreateResponse(new List<BooksViewModel>(), HttpStatusCode.NotFound, false, "Some books are not found.");
            }
        }

        bool bIsSuccess = await _bookRepository.UpdateRangeAsync(listBooks);

        if (bIsSuccess)
        {
            List<BooksViewModel> updatedBooks = await _bookRepository.GetBooksDtoByIds(listBooksViewModels.Select(b => b.Id).ToList());
            return CommonHelper.CreateResponse(updatedBooks, HttpStatusCode.OK, true, "All selected Books has been updated successfully");
        }

        return CommonHelper.CreateResponse(new List<BooksViewModel>(), HttpStatusCode.BadRequest, false, "Some error occured while updating the books.");
    }

    public async Task<Response<bool>> DeleteBook(long id)
    {
        Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(id);

        if (objExistingBook != null)
        {
            bool bIsSuccess = await _bookRepository.DeleteAsync(objExistingBook);

            if (bIsSuccess)
            {
                return CommonHelper.CreateResponse(false, HttpStatusCode.OK, true, "Book Deleted successfully");
            }
        }

        return CommonHelper.CreateResponse(false, HttpStatusCode.NotFound, false, "Book not found.");
    }

    public async Task<Response<bool>> DeleteListOfBooks(List<int> ids)
    {
        List<Book> listBooks = new List<Book>();

        foreach (var id in ids)
        {
            Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(id);

            if (objExistingBook != null)
            {
                listBooks.Add(objExistingBook);
            }
            else
            {
                return CommonHelper.CreateResponse(false, HttpStatusCode.NotFound, false, "Some books are not found.");
            }
        }

        bool bIsSuccess = await _bookRepository.DeleteRangeAsync(listBooks);

        if (bIsSuccess)
        {
            return CommonHelper.CreateResponse(false, HttpStatusCode.OK, true, "All selected Books has been deleted successfully");
        }

        return CommonHelper.CreateResponse(false, HttpStatusCode.OK, false, "Some error occured while deleting the books.");
    }

}
