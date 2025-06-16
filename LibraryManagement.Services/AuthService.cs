using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagement.Services;

public class AuthService : IAuthService
{

    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    public AuthService(IUserRepository userRepository, IConfiguration configuration) {
        _userRepository = userRepository;
        _configuration = configuration;
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
            Password = HashPassword(objUserRegisterViewModel.Password),
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
                Password = objUserRegisterViewModel.Password,
                Role = u.Role,
            }
        );

        return CommonHelper.CreateResponse(objAddedUser, HttpStatusCode.OK, true, "User has been added successfully.");

    }

    public async Task<Response<AuthResultViewModel>> LoginUser(UserLoginViewModel objUserLoginViewModel)
    {
        User? objExistingUser = await _userRepository.GetFirstOrDefaultAsync(
            u => u.Email == objUserLoginViewModel.Email && 
            u.Password == HashPassword(objUserLoginViewModel.Password),
            u => new User {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role
            }
        );

        if(objExistingUser == null) {
            return CommonHelper.CreateResponse(new AuthResultViewModel(), HttpStatusCode.NotFound, false, "Invalid credentials.");
        }

        AuthResultViewModel? objAuthResult = new AuthResultViewModel{
            AccessToken = GenerateJWTToken(objExistingUser)
        };

        return CommonHelper.CreateResponse(objAuthResult, HttpStatusCode.OK, true, "User logged in successfully.");

    }

    private string HashPassword(string sPassword)
    {
        string _salt = _configuration["PasswordSecurity:PasswordSalt"]!;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_salt));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(sPassword));
        return Convert.ToBase64String(hashBytes);
    }

    private string GenerateJWTToken(User objUser)
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = new[] {
            new Claim(ClaimTypes.Role, objUser.Role.ToString()),
            new Claim(ClaimTypes.Email, objUser.Email),
        };

        JwtSecurityToken tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration["JwtConfig:Issuer"],
            audience: _configuration["JwtConfig:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
