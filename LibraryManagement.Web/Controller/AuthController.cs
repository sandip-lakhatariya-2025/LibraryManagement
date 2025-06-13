using Asp.Versioning;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controller;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(2)]
public class AuthController : ControllerBase
{

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="objUserRegisterViewModel"></param>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     POST /api/v2/User/Register
    ///     {
    ///         "email": "john.doe@example.com",
    ///         "firstName": "John",
    ///         "lastName": "Doe",
    ///         "password": "SecurePassword123!",
    ///         "role": "admin"
    ///     }
    ///
    /// </remarks>

    [MapToApiVersion(2)]
    [HttpPost]
    public async Task<IActionResult> Register(UserRegisterViewModel objUserRegisterViewModel) {
        var response = await _authService.RegisterUser(objUserRegisterViewModel);
        return response.Succeeded 
            ? Ok(response)
            : StatusCode((int)response.StatusCode, response);
    }
}
