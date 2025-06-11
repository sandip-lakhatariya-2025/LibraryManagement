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
    private const string defaultHeaderKey = "X-Idempotency-Key";
    
    private readonly TimeSpan _cacheDuration;
    private readonly string _headerKeyName;
    private readonly bool _isEnabled;

    public IdempotentAttribute(
        int cacheTimeInMinutes = defaultCacheTimeInMinutes,
        string headerKeyName = defaultHeaderKey,
        bool isEnabled = true)
    {
        _cacheDuration = TimeSpan.FromMinutes(cacheTimeInMinutes);
        _headerKeyName = headerKeyName;
        _isEnabled = isEnabled;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!_isEnabled || !HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(_headerKeyName, out StringValues idempotencyKey))
        {
            context.Result = new BadRequestObjectResult($"Invalid or missing {_headerKeyName} header");
            return;
        }

        IDistributedCache cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        string cacheKey = $"Idempotent_{idempotencyKey}";
        string? cachedResult = await cache.GetStringAsync(cacheKey);

        if (cachedResult is not null)
        {
            string path = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "unknown";
            var idempotentResponse = JsonSerializer.Deserialize<IdempotentResponse>(cachedResult)!;

            if (!path.Equals(idempotentResponse.Path, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new BadRequestObjectResult(
                    $"The idempotency key '{idempotencyKey}' was used in a different request path."
                );
            }
            else
            {
                Response<bool> response = CommonHelper.CreateResponse(
                    false,
                    HttpStatusCode.BadRequest,
                    false,
                    $"The idempotency key '{idempotencyKey}' was already used."
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

            IdempotentResponse response = new(statusCode, objectResult.Value, path);

            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration
                }
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
