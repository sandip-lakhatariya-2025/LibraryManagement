using LibraryManagement.Common;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controller;

[ApiController]
[Route("api/[Controller]")]
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

    [HttpGet]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Read)]

    public async Task<ActionResult<List<BooksViewModel>>> GetBooks([FromQuery] PaginationFilter paginationFilter, [FromQuery] Dictionary<string, string>? filters, string? fields)
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

    [HttpGet("{id}")]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Read)]
    public async Task<ActionResult<BooksViewModel>> GetBook(long id)
    {
        BooksViewModel? booksViewModel = await _bookService.GetBookById(id);

        if (booksViewModel == null)
        {
            return NotFound();
        }

        return booksViewModel;
    }

    /// <summary>
    /// Add new Book record.
    /// </summary>

    [HttpPost]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Write)]
    [Idempotent]
    public async Task<IActionResult> AddBook(BooksViewModel booksViewModel)
    {
        var response = await _bookService.AddBook(booksViewModel);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Update the Book record.
    /// </summary>

    [HttpPut("{id}")]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Update)]
    public async Task<IActionResult> UpdateBook(long id, BooksViewModel booksViewModel)
    {

        if (id != booksViewModel.Id)
        {
            return BadRequest();
        }

        var response = await _bookService.UpdateBook(booksViewModel);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Update the List of Book records.
    /// </summary>

    [HttpPut]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.MultipleUpdate)]
    public async Task<IActionResult> UpdateBooks(List<BooksViewModel> booksViewModels)
    {

        var response = await _bookService.UpdateListOfBooks(booksViewModels);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Delete the Book record.
    /// </summary>

    [HttpDelete("{id}")]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.Delete)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var result = await _bookService.DeleteBook(id);

        if (result.isSuccess)
        {
            return Ok(new { message = result.message });
        }

        return BadRequest(new { error = result.message });
    }

    /// <summary>
    /// Delete the List of Book records.
    /// </summary>

    [HttpDelete]
    [PermissionAuthorize(ClientEndpoint.Book, Permission.MultipleDelete)]
    public async Task<IActionResult> DeleteBooks(List<int> ids)
    {
        var result = await _bookService.DeleteListOfBooks(ids);

        if (result.isSuccess)
        {
            return Ok(new { message = result.message });
        }

        return BadRequest(new { error = result.message });
    }
}
