using System.Dynamic;
using System.Net;
using AutoMapper;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.IRepository;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;
using LibraryManagement.Services.IServices;
using LibraryManagement.Utility;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public BookService(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooksV1(PaginationFilter paginationFilter, IQueryCollection queryParams, string? sFields)
    {
        List<FilterCriteria> filters = FilterParser.ParseFiltersV1(queryParams);
        var (lstBooks, nTotalRecords) = await _bookRepository.GetPaginatedListAsyncV1(paginationFilter, filters);
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

    public async Task<PagedResponse<List<ExpandoObject>>> GetPaginatedListOfBooksV2(PaginationFilter paginationFilter, string? sFilters, string? sFields)
    {
        List<List<FilterCriteria>> filters = FilterParser.ParseFiltersV2(sFilters);
        var (lstBooks, nTotalRecords) = await _bookRepository.GetPaginatedListAsyncV2(paginationFilter, filters);
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

    public async Task<BookResultDto?> GetBookById(long id)
    {
        return await _bookRepository.GetFirstOrDefaultAsync<BookResultDto>(b => b.Id == id, _mapper.ConfigurationProvider);
    }

    public async Task<Response<ExpandoObject>> GetBookByIdV2(long id, string? sFields)
    {
        BookResultDto? objBookDto = await _bookRepository.GetFirstOrDefaultAsync<BookResultDto>(b => b.Id == id, _mapper.ConfigurationProvider);

        if(objBookDto == null) {
            return CommonHelper.CreateResponse(new ExpandoObject(), HttpStatusCode.NotFound, false, ResponseMessages.NotFound.With("Book"));
        }

        var shapedBook = ObjectShaper.GetShapedObject(objBookDto, sFields);

        return CommonHelper.CreateResponse(shapedBook, HttpStatusCode.OK, true, $"Book details fetch successfully.");
    }

    public async Task<Response<BookResultDto?>> AddBook(BookCreateDto objBookCreateDto)
    {
        Book objNewBook = _mapper.Map<Book>(objBookCreateDto);

        await _bookRepository.InsertAsync(objNewBook);
        await _bookRepository.SaveChangesAsync();

        BookResultDto? addedBook = await GetBookById(objNewBook.Id);
        return CommonHelper.CreateResponse(addedBook, HttpStatusCode.OK, true, "Book added successfully.");
    }

    public async Task<Response<BookResultDto?>> UpdateBook(BookUpdateDto objBookUpdateDto)
    {
        Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(b => b.Id == objBookUpdateDto.Id);

        if (objExistingBook != null)
        {
            _mapper.Map(objBookUpdateDto, objExistingBook);

            _bookRepository.UpdateEntity(objExistingBook);
            await _bookRepository.SaveChangesAsync();

            BookResultDto? updatedBook = await GetBookById(objExistingBook.Id);

            return CommonHelper.CreateResponse(updatedBook, HttpStatusCode.OK, true, "Book Updated successfully");
        }

        return CommonHelper.CreateResponse<BookResultDto?>(null, HttpStatusCode.NotFound, false, "Book not found.");
    }

    public async Task<Response<List<BookResultDto>>> UpdateListOfBooks(List<BookUpdateDto> lstBookUpdateDtos)
    {
        List<Book> listBooks = new List<Book>();

        foreach (var objBookUpdateDto in lstBookUpdateDtos)
        {
            Book? objExistingBook = await _bookRepository.GetFirstOrDefaultAsync(b => b.Id == objBookUpdateDto.Id);

            if (objExistingBook != null)
            {
                _mapper.Map(objBookUpdateDto, objExistingBook);

                listBooks.Add(objExistingBook);
            }
            else
            {
                return CommonHelper.CreateResponse(new List<BookResultDto>(), HttpStatusCode.NotFound, false, "Some books are not found.");
            }
        }

        _bookRepository.UpdateList(listBooks);
        await _bookRepository.SaveChangesAsync();

        List<long> lstIds = lstBookUpdateDtos.Select(b => b.Id).ToList();
        List<BookResultDto> updatedBooks = await _bookRepository.GetListAsync<BookResultDto>(
            b => lstIds.Contains(b.Id),
            _mapper.ConfigurationProvider
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
