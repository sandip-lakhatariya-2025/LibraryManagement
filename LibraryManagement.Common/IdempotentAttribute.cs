using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace LibraryManagement.Common;

public class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    private const int defaultCacheTimeInMinutes = 60;
    private readonly TimeSpan _cacheDuration;

    public IdempotentAttribute(int cacheTimeInMinutes = defaultCacheTimeInMinutes) 
    {
        _cacheDuration = TimeSpan.FromMinutes(cacheTimeInMinutes);
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) 
    {
        if(!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out StringValues idempotencyKeyValue) || 
            !Guid.TryParse(idempotencyKeyValue, out Guid idempotencyKey)) 
        {
            context.Result = new BadRequestObjectResult("Invalid or missing X-Idempotency-Key header");
            return;
        }

        IDistributedCache cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        string path = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "unknown";
        string cacheKey = $"Idempotent_{path}_{idempotencyKey}";
        // string cacheKey = $"Idempotent_{idempotencyKey}";
        string? cachedResult = await cache.GetStringAsync(cacheKey);
        if(cachedResult is not null) {
            IdempotentResponse idempotentResponse = JsonSerializer.Deserialize<IdempotentResponse>(cachedResult)!;

            var result = new ObjectResult(idempotentResponse.Value) {
                StatusCode = 400
            };

            context.Result = result;
            
            // context.Result = new BadRequestObjectResult("Duplicate request: This operation has already been processed.");
            return;
        }

        ActionExecutedContext executedContext = await next();

        if (executedContext.Result is ObjectResult { StatusCode: >= 200 and < 300 } objectResult)
        {
            int statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            IdempotentResponse response = new(statusCode, objectResult.Value);

            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheDuration }
            );
        }
    }
}

internal sealed class IdempotentResponse
{
    [JsonConstructor]
    public IdempotentResponse(int statusCode, object? value)
    {
        StatusCode = statusCode;
        Value = value;
    }

    public int StatusCode { get; }
    public object? Value { get; }
}
