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
        List<FilterCriteria> filters = FilterParser.ParseFiltersV1(queryParams);
        var (lstBooks, nTotalRecords) = await _bookRepository.GetPaginatedListAsync(paginationFilter, filters);
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

    public async Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooks(PaginationFilter paginationFilter, string? sFilters, string? sFields)
    {
        List<FilterCriteria> filters = FilterParser.ParseFiltersV2(sFilters);
        // List<List<FilterCriteria>> filters = FilterParser.ParseGroupedFilters(sFilters);
        var (lstBooks, nTotalRecords) = await _bookRepository.GetPaginatedListAsync(paginationFilter, filters);
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
        return await _bookRepository.GetFirstOrDefaultAsync(
            b => b.Id == id, 
            b => new BooksViewModel {
                Id = b.Id,
                BookName = b.BookName,
                AutherName = b.AutherName,
                Description = b.Description,
                Price = b.Price,
                TotalCopies = b.TotalCopies,
                PublisherId = b.PublisherId
            }
        );
    }

    public async Task<Response<ExpandoObject>> GetBookByIdV2(long id, string? sFields)
    {
        BooksViewModel? objBookViewModel = await _bookRepository.GetFirstOrDefaultAsync(
            b => b.Id == id, 
            b => new BooksViewModel {
                Id = b.Id,
                BookName = b.BookName,
                AutherName = b.AutherName,
                Description = b.Description,
                Price = b.Price,
                TotalCopies = b.TotalCopies,
                PublisherId = b.PublisherId
            }
        );

        if(objBookViewModel == null) {
            return CommonHelper.CreateResponse(new ExpandoObject(), HttpStatusCode.NotFound, false, ResponseMessages.NotFound.With("Book"));
        }

        var shapedBook = ObjectShaper.GetShapedObject(objBookViewModel, sFields);

        return CommonHelper.CreateResponse(shapedBook, HttpStatusCode.OK, true, $"Book details fetch successfully.");
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

        await _bookRepository.InsertAsync(objNewBook);
        await _bookRepository.SaveChangesAsync();

        BooksViewModel? addedBook = await GetBookById(objNewBook.Id);
        return CommonHelper.CreateResponse(addedBook, HttpStatusCode.OK, true, "Book added successfully.");
    }

    public async Task<Response<BooksViewModel?>> UpdateBook(BooksViewModel objBookViewModel)
    {
        Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(b => b.Id == objBookViewModel.Id);

        if (objExistingBook != null)
        {
            objExistingBook.BookName = objBookViewModel.BookName;
            objExistingBook.AutherName = objBookViewModel.AutherName;
            objExistingBook.Description = objBookViewModel.Description;
            objExistingBook.Price = objBookViewModel.Price;
            objExistingBook.TotalCopies = objBookViewModel.TotalCopies;

            _bookRepository.UpdateEntity(objExistingBook);
            await _bookRepository.SaveChangesAsync();

            BooksViewModel? updatedBook = await GetBookById(objExistingBook.Id);

            return CommonHelper.CreateResponse(updatedBook, HttpStatusCode.OK, true, "Book Updated successfully");
        }

        return CommonHelper.CreateResponse<BooksViewModel?>(null, HttpStatusCode.NotFound, false, "Book not found.");
    }

    public async Task<Response<List<BooksViewModel>>> UpdateListOfBooks(List<BooksViewModel> lstBooksViewModels)
    {
        List<Book> listBooks = new List<Book>();

        foreach (var objBookViewModel in lstBooksViewModels)
        {
            Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(b => b.Id == objBookViewModel.Id);

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

        _bookRepository.UpdateList(listBooks);
        await _bookRepository.SaveChangesAsync();

        List<long> lstIds = lstBooksViewModels.Select(b => b.Id).ToList();
        List<BooksViewModel> updatedBooks = await _bookRepository.GetListAsync(
            b => lstIds.Contains(b.Id),
            b => new BooksViewModel {
                Id = b.Id,
                BookName = b.BookName,
                AutherName = b.AutherName,
                Description = b.Description,
                Price = b.Price,
                TotalCopies = b.TotalCopies
            }    
        );

        return CommonHelper.CreateResponse(updatedBooks, HttpStatusCode.OK, true, "All selected Books has been updated successfully");
    }

    public async Task<Response<bool>> DeleteBook(long id)
    {
        Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(b => b.Id == id);

        if (objExistingBook != null)
        {
            _bookRepository.DeleteEntity(objExistingBook);
            await _bookRepository.SaveChangesAsync();

            return CommonHelper.CreateResponse(false, HttpStatusCode.OK, true, "Book Deleted successfully");
        }

        return CommonHelper.CreateResponse(false, HttpStatusCode.NotFound, false, ResponseMessages.NotFound.With("Book"));
    }

    public async Task<Response<bool>> DeleteListOfBooks(List<int> ids)
    {
        List<Book> listBooks = new List<Book>();

        foreach (var id in ids)
        {
            Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(b => b.Id == id);

            if (objExistingBook != null)
            {
                listBooks.Add(objExistingBook);
            }
            else
            {
                return CommonHelper.CreateResponse(false, HttpStatusCode.NotFound, false, "Some books are not found.");
            }
        }

        _bookRepository.DeleteList(listBooks);
        await _bookRepository.SaveChangesAsync();

        return CommonHelper.CreateResponse(false, HttpStatusCode.OK, true, "All selected Books has been deleted successfully");
    }

}
