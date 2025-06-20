using LibraryManagement.Common;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;

namespace LibraryManagement.Services.IServices;

public interface IAuthService
{
    Task<Response<RegistrationResultDto?>> RegisterUser(RegisterDto objRegisterDto);
    Task<Response<AuthResultDto>> LoginUser(LoginDto objLoginDto);
}
