using LibraryManagement.Common;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controller;

[ApiController]
[Route("api/[Controller]")]
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
    /// <param name="customerViewModel">The customer details to add.</param>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     POST /api/Customer/
    ///     {
    ///         "id": 101,
    ///         "Name": "John",
    ///         "email": "john.doe@example.com",
    ///     }
    ///
    /// </remarks>

    [HttpPost]
    [PermissionAuthorize(ClientEndpoint.Customer, Permission.Write)]
    [Idempotent]
    public async Task<IActionResult> AddCustomer(CustomerViewModel customerViewModel)
    {
        var response = await _customerService.AddCustomer(customerViewModel);

        if (response.Succeeded)
        {
            return Ok(response);
        }
        
        return StatusCode((int)response.StatusCode, response);
    }
}
