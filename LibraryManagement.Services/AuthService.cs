using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagement.Services;

public class AuthService : IAuthService
{

    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    public AuthService(IUserRepository userRepository, IConfiguration configuration, IMapper mapper) {
        _userRepository = userRepository;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<Response<RegistrationResultDto?>> RegisterUser(RegisterDto objRegisterDto)
    {
        bool bIsExist = await _userRepository.ExistAsync(u => u.Email == objRegisterDto.Email);

        if(bIsExist) {
            return CommonHelper.CreateResponse<RegistrationResultDto?>(null, HttpStatusCode.BadRequest, true, "User with this email already exist.");
        }

        User objNewUser = _mapper.Map<User>(objRegisterDto);

        await _userRepository.InsertAsync(objNewUser);
        await _userRepository.SaveChangesAsync();

        RegistrationResultDto? objAddedUser = await _userRepository.GetFirstOrDefaultAsync<RegistrationResultDto>(
            u => u.Id == objNewUser.Id,
            _mapper.ConfigurationProvider
        );

        return CommonHelper.CreateResponse(objAddedUser, HttpStatusCode.OK, true, "User has been added successfully.");

    }

    public async Task<Response<AuthResultDto>> LoginUser(LoginDto objLoginDto)
    {
        User? objExistingUser = await _userRepository.GetFirstOrDefaultAsync<User>(
            u => u.Email == objLoginDto.Email && 
            u.Password == HashPassword(objLoginDto.Password),
            _mapper.ConfigurationProvider
        );

        if(objExistingUser == null) {
            return CommonHelper.CreateResponse(new AuthResultDto(), HttpStatusCode.NotFound, false, "Invalid credentials.");
        }

        AuthResultDto? objAuthResult = new AuthResultDto{
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
