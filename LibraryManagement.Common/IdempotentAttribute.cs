using System.Net;
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
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out StringValues idempotencyKeyValue) ||
            !Guid.TryParse(idempotencyKeyValue, out Guid idempotencyKey)) 
        {
            context.Result = new BadRequestObjectResult("Invalid or missing X-Idempotency-Key header");
            return;
        }

        IDistributedCache cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        string cacheKey = $"Idempotent_{idempotencyKey}";
        string? cachedResult = await cache.GetStringAsync(cacheKey);

        if (cachedResult is not null)
        {
            string path = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "unknown";
            var idempotentResponse = JsonSerializer.Deserialize<IdempotentResponse>(cachedResult)!;

            if(!path.Equals(idempotentResponse.Path, StringComparison.OrdinalIgnoreCase)) {
                context.Result = new BadRequestObjectResult(
                    $"The Idempotency header key value '{idempotencyKey}' was used in a different request."
                );
            }
            else {
                Response<bool> response = CommonHelper.CreateResponse(
                    false, 
                    HttpStatusCode.BadRequest, 
                    false, 
                    $"The Idempotency header key value '{idempotencyKey}' was used in a different request."
                );

                context.Result = new BadRequestObjectResult(response);
            }

            return;
        }

        ActionExecutedContext executedContext = await next();

        if (executedContext.Result is ObjectResult { StatusCode: >= 200 and < 300 } objectResult)
        {
            int statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            string path = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "unknown";
            string method = context.HttpContext.Request.Method;

            IdempotentResponse response = new(statusCode, objectResult.Value, path);

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
    public IdempotentResponse(int statusCode, object? value, string path)
    {
        StatusCode = statusCode;
        Value = value;
        Path = path;
    }

    public int StatusCode { get; }
    public object? Value { get; }
    public string Path { get; }
}
