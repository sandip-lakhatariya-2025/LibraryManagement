using System.Net;
using System.Security.Cryptography;
using System.Text;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Configuration;

namespace LibraryManagement.Services;

public class AuthService : IAuthService
{

    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository) {
        _userRepository = userRepository;
    }

    public async Task<Response<UserRegisterViewModel?>> RegisterUser(UserRegisterViewModel objUserRegisterViewModel)
    {
        bool bIsExist = await _userRepository.ExistAsync(u => u.Email == objUserRegisterViewModel.Email);

        if(bIsExist) {
            return CommonHelper.CreateResponse<UserRegisterViewModel?>(null, HttpStatusCode.NotFound, false, "User with this email already exist.");
        }

        User objNewUser = new User {
            Email = objUserRegisterViewModel.Email,
            Firstname = objUserRegisterViewModel.Firstname,
            Lastname = objUserRegisterViewModel.Lastname,
            Password = objUserRegisterViewModel.Password,
            Role = objUserRegisterViewModel.Role
        };

        await _userRepository.InsertAsync(objNewUser);
        await _userRepository.SaveChangesAsync();

        UserRegisterViewModel? objAddedUser = await _userRepository.GetFirstOrDefaultAsync(
            u => u.Id == objNewUser.Id,
            u => new UserRegisterViewModel {
                Id = u.Id,
                Email = u.Email,
                Firstname = u.Firstname,
                Lastname = u.Lastname,
                Password = u.Password,
                Role = u.Role,
            }
        );

        return CommonHelper.CreateResponse(objAddedUser, HttpStatusCode.OK, true, "User has been added successfully.");

    }

    public Task<Response<UserLoginViewModel?>> LoginUser(UserLoginViewModel objUserLoginViewModel)
    {
        throw new NotImplementedException();
    }

    // private string HashApiKey(string apiKey)
    // {
    //     string _salt = configuration["ApiSecurity:ApiKeySalt"]
    //     using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_salt));
    //     var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
    //     return Convert.ToBase64String(hashBytes);
    // }
}
