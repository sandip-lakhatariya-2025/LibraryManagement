using Asp.Versioning;
using LibraryManagement.Common;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controller;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2")]
[Authorize(Roles = "Admin")]
public class CustomerController : ControllerBase
{

    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Adds a new customer.
    /// </summary>
    /// <param name="objCustomerCreateDto">The customer details to add.</param>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     POST /api/v2/Customer/
    ///     {
    ///         "name": "John",
    ///         "email": "john.doe@example.com",
    ///     }
    ///
    /// </remarks>

    [MapToApiVersion("2")]
    [HttpPost]
    [PermissionAuthorize(ClientEndpoint.Customer, Permission.Write)]
    [Idempotent]
    public async Task<IActionResult> AddCustomer(CustomerCreateDto objCustomerCreateDto)
    {
        var response = await _customerService.AddCustomer(objCustomerCreateDto);

        if (response.Succeeded)
        {
            return Ok(response);
        }
        
        return StatusCode((int)response.StatusCode, response);
    }
}
