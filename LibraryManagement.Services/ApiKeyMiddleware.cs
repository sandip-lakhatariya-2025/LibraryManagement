using LibraryManagement.DataAccess.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.Services;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _storedHashedApiKey;
    private readonly string _salt;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _next = next;
        _salt = configuration["ApiSecurity:ApiKeySalt"]
                ?? throw new ArgumentException("API key salt not configured.");

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _storedHashedApiKey = dbContext.Settings
                                .Where(s => s.SettingName == "API-Key")
                                .Select(s => s.SettingValue)
                                .FirstOrDefault();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key was not provided.");
            return;
        }

        var hashedInput = HashApiKey(extractedApiKey!);

        if (_storedHashedApiKey == null || !_storedHashedApiKey.Equals(hashedInput))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized client.");
            return;
        }

        await _next(context);
    }

    private string HashApiKey(string apiKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_salt));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes);
    }
}
