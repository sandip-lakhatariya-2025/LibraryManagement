using Asp.Versioning;
using LibraryManagement.Common;
using LibraryManagement.Models.Dtos.RequestDtos;
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
    /// <param name="objRegisterDto"></param>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     POST /api/v2/Auth/Register
    ///     {
    ///         "email": "john.doe@example.com",
    ///         "firstName": "John",
    ///         "lastName": "Doe",
    ///         "password": "SecurePassword123!",
    ///         "role": "Admin"
    ///     }
    ///
    /// </remarks>

    [MapToApiVersion(2)]
    [HttpPost("Register")]
    [Idempotent(cacheTimeInMinutes:60, headerKeyName: "X-Idempotency-Key", isEnabled: true)]
    public async Task<IActionResult> Register(RegisterDto objRegisterDto) {
        var response = await _authService.RegisterUser(objRegisterDto);
        return response.Succeeded 
            ? Ok(response)
            : StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Login to the system.
    /// </summary>
    /// <param name="objLoginDto"></param>
    /// <remarks>
    /// **Sample request body:**
    ///
    ///     POST /api/v2/Auth/Login
    ///     {
    ///         "email": "john.doe@example.com",
    ///         "password": "SecurePassword123!",
    ///     }
    ///
    /// </remarks>

    [MapToApiVersion(2)]
    [HttpPost("Login")]
    [Idempotent(cacheTimeInMinutes:60, headerKeyName: "X-Idempotency-Key", isEnabled: true)]
    public async Task<IActionResult> Login(LoginDto objLoginDto) {
        var response = await _authService.LoginUser(objLoginDto);
        return response.Succeeded 
            ? Ok(response)
            : StatusCode((int)response.StatusCode, response);
    }
}
