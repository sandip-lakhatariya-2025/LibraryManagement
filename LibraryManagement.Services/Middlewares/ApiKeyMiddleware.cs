using LibraryManagement.DataAccess.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.Services;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _salt;
    private readonly IServiceScopeFactory _scopeFactory;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _salt = configuration["ApiSecurity:ApiKeySalt"]
                ?? throw new ArgumentException("API key salt not configured.");
        _scopeFactory = scopeFactory;
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

        ApiKeyInfo? apiKeyInfo;

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            apiKeyInfo = await dbContext.Settings
                .Where(s => s.SettingName == "API-Key")
                .Select(s => new ApiKeyInfo
                {
                    Key = s.SettingValue,
                    RequestPerMinute = s.RequestPerMinute
                })
                .FirstOrDefaultAsync();
        }

        if (apiKeyInfo == null || !apiKeyInfo.Key!.Equals(hashedInput))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized client.");
            return;
        }

        context.Items["ApiKey"] = apiKeyInfo.Key;
        context.Items["RateLimitPerMinute"] = apiKeyInfo.RequestPerMinute;

        await _next(context);
    }

    private string HashApiKey(string apiKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_salt));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes);
    }
}

internal sealed class ApiKeyInfo 
{
    public string? Key { get; set; }
    public int RequestPerMinute { get; set; }
}
