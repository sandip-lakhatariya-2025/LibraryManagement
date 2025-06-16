using LibraryManagement.Common;
using LibraryManagement.Models.ViewModels;

namespace LibraryManagement.Services.IServices;

public interface IAuthService
{
    Task<Response<UserRegisterViewModel?>> RegisterUser(UserRegisterViewModel objUserRegisterViewModel);
    Task<Response<AuthResultViewModel>> LoginUser(UserLoginViewModel objUserLoginViewModel);
}
