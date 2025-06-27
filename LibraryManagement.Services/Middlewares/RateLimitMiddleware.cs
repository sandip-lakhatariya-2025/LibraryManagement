using System.Net;
using System.Text.Json;
using LibraryManagement.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace LibraryManagement.Services;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public RateLimitMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? sApiKey = context.Items["ApiKey"]?.ToString();
        int nLimit = context.Items["RateLimitPerMinute"] as int? ?? 10;

        string sCacheKey = $"ratenLimit_{sApiKey}";

        DateTime currentTime = DateTime.UtcNow;
        string? sCachedData = await _cache.GetStringAsync(sCacheKey);
        RateLimitInfo rateLimitInfo;
        
        if (string.IsNullOrEmpty(sCachedData))
        {
            rateLimitInfo = new RateLimitInfo
            {
                RequestCount = 1,
                WindowStart = currentTime
            };
        }
        else
        {
            rateLimitInfo = JsonSerializer.Deserialize<RateLimitInfo>(sCachedData)!;

            if (rateLimitInfo.WindowEnd <= currentTime)
            {
                rateLimitInfo.RequestCount = 1;
                rateLimitInfo.WindowStart = currentTime;
            }
            else
            {
                rateLimitInfo.RequestCount++;
            }
        }

        int remainingLimit = nLimit - rateLimitInfo.RequestCount;

        context.Response.Headers["X-RateLimit-Limit"] = nLimit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, remainingLimit).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(rateLimitInfo.WindowEnd).ToUnixTimeSeconds().ToString();

        if (rateLimitInfo.RequestCount > nLimit)
        {
            context.Response.Headers["Retry-After"] = Math.Ceiling((rateLimitInfo.WindowEnd - currentTime).TotalSeconds).ToString();
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = CommonHelper.CreateResponse(
                data: false,
                httpStatuscode: HttpStatusCode.TooManyRequests,
                bIsSuccess: false,
                sMessage: "Rate limit exceeded. Please try again later."
            );

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        var expiration = rateLimitInfo.WindowEnd - currentTime;

        await _cache.SetStringAsync(
            sCacheKey,
            JsonSerializer.Serialize(rateLimitInfo),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            }
        );

        await _next(context);
    }
}

internal sealed class RateLimitInfo
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd => WindowStart.AddMinutes(1);
}