using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using LibraryManagement.Common;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controller;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1")]
[ApiVersion("2")]
public class BookController : ControllerBase
{

    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Retrieves a paginated list of books.
    /// </summary>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     GET /api/v1/Book/
    /// 
    /// </remarks>
    
    [MapToApiVersion("1")]
    [HttpGet]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Read)]
    public async Task<IActionResult> GetBooksV1([FromQuery] PaginationFilter paginationFilter, [FromQuery] Dictionary<string, string>? filters, string? fields)
    {

        var response = await _bookService.GetPaginatedListOfBooks(paginationFilter, Request.Query, fields);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a book record by Id.
    /// </summary>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     GET /api/v1/Book/1
    /// 
    /// </remarks>
    
    [MapToApiVersion("1")]
    [HttpGet("{id}")]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Read)]
    public async Task<IActionResult> GetBookV1(long id)
    {
        BooksViewModel? objBook = await _bookService.GetBookById(id);

        if (objBook == null)
        {
            return NotFound();
        }

        return Ok(objBook);
    }

    /// <summary>
    /// Retrieves a book record by Id or a paginated list of books.
    /// </summary>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     GET /api/v2/Book/
    ///     GET /api/v2/Book/1
    /// 
    /// </remarks>
    
    [MapToApiVersion("2")]
    [HttpGet]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Read)]
    public async Task<IActionResult> GetBooksV2(int id, [FromQuery] PaginationFilter paginationFilter, [FromQuery] Dictionary<string, string>? filters, string? fields)
    {
        if (id <= 0)
        {
            var pagedResponse = await _bookService.GetPaginatedListOfBooks(paginationFilter, Request.Query, fields);
            return pagedResponse.Succeeded
                ? Ok(pagedResponse)
                : StatusCode((int)pagedResponse.StatusCode, pagedResponse);
        }

        var singleResponse = await _bookService.GetBookByIdV2(id, fields);
        return singleResponse.Succeeded
            ? Ok(singleResponse)
            : StatusCode((int)singleResponse.StatusCode, singleResponse);
    }

    /// <summary>
    /// Add new Book record.
    /// </summary>
    /// <param name="objBookViewModel"></param>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     POST /api/v2/Book/
    ///     {
    ///         "bookName": "Atomic Habits",
    ///         "autherName": "James Clear",
    ///         "description": "A guide to building better habits.",
    ///         "price": 750,
    ///         "totalCopies": 120,
    ///         "publisherId": 2
    ///     }
    ///
    /// </remarks>
    
    [MapToApiVersion("2")]
    [HttpPost]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Write)]
    [Idempotent(cacheTimeInMinutes:60, headerKeyName: "X-Idempotency-Key", isEnabled: true)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<IActionResult> AddBook(BooksViewModel objBookViewModel)
    {
        var response = await _bookService.AddBook(objBookViewModel);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Updates a book by its ID.
    /// </summary>
    /// <param name="id">The ID of the book to update.</param>
    /// <param name="objBookViewModel">The updated book details.</param>
    /// <remarks>
    /// **Sample request:**
    ///
    ///     PUT /api/v2/Book/8
    ///     {
    ///         "id": 8,
    ///         "bookName": "Atomic Habits",
    ///         "autherName": "James Clear",
    ///         "description": "A guide to building better habits.",
    ///         "price": 750,
    ///         "totalCopies": 120,
    ///         "publisherId": 2
    ///     }
    ///
    /// </remarks>

    [MapToApiVersion("2")]
    [HttpPut("{id}")]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Update)]
    public async Task<IActionResult> UpdateBook(long id, BooksViewModel objBookViewModel)
    {

        if (id != objBookViewModel.Id)
        {
            return BadRequest();
        }

        var response = await _bookService.UpdateBook(objBookViewModel);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Updates multiple book records.
    /// </summary>
    /// <param name="listBooksViewModels">A list of book view models to update.</param>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     PUT /api/v2/Book/
    ///     [
    ///         {
    ///             "id": 7,
    ///             "bookName": "The 7 Habits of Highly Effective People",
    ///             "autherName": "Stephen R. Covey",
    ///             "description": "A powerful book on effectiveness.",
    ///             "price": 890,
    ///             "totalCopies": 100,
    ///             "publisherId": 1
    ///         },
    ///         {
    ///             "id": 8,
    ///             "bookName": "Atomic Habits",
    ///             "autherName": "James Clear",
    ///             "description": "A guide to building better habits.",
    ///             "price": 750,
    ///             "totalCopies": 120,
    ///             "publisherId": 2
    ///         }
    ///     ]
    /// </remarks>
    
    [MapToApiVersion("2")]
    [HttpPut]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.MultipleUpdate)]
    public async Task<IActionResult> UpdateBooks(List<BooksViewModel> listBooksViewModels)
    {

        var response = await _bookService.UpdateListOfBooks(listBooksViewModels);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a book by its ID.
    /// </summary>
    /// <param name="id">The ID of the book to delete.</param>
    /// <remarks>
    /// **Sample request:**
    ///
    ///     DELETE /api/v2/Book/8
    ///
    /// </remarks>
    
    [MapToApiVersion("2")]
    [HttpDelete("{id}")]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Delete)]
    public async Task<IActionResult> DeleteBook(long id)
    {
        var response = await _bookService.DeleteBook(id);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Deletes multiple books by their IDs.
    /// </summary>
    /// <param name="lstIds">A list of book IDs to delete.</param>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     DELETE /api/v2/Book/
    ///     [8, 9, 10]
    ///
    /// </remarks>
    
    [MapToApiVersion("2")]
    [HttpDelete]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.MultipleDelete)]
    public async Task<IActionResult> DeleteBooks(List<int> lstIds)
    {
        var response = await _bookService.DeleteListOfBooks(lstIds);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }
}
