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
    /// Add new Customer record.
    /// </summary>

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
