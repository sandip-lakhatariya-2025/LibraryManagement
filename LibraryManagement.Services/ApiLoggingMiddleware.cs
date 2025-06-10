using System.Diagnostics;
using LibraryManagement.DataAccess.Data;
using LibraryManagement.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiLoggingMiddleware> _logger;

    public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var sw = Stopwatch.StartNew();
        string sMethodName = context.Request.Method;
        string sControllerName = context.GetRouteValue("controller")?.ToString() ?? "Unknown";
        string sActionName = context.GetRouteValue("action")?.ToString() ?? "Unknown";
        var path = context.Request.Path;
        string sRoute = $"{sControllerName}/{sActionName}";

        var logEntry = new APIStackTrace
        {
            EndpointName = path,
            RequestRoute = sRoute,
            ApiType = sMethodName,
            IsSuccess = false,
            CreatedAt = DateTime.UtcNow
        };

        db.APIStackTraces.Add(logEntry);
        await db.SaveChangesAsync();

        try
        {
            _logger.LogInformation("API Call Started => Route: {Controller}/{Action}", sControllerName, sActionName);
            
            await _next(context);
            logEntry.IsSuccess = context.Response.StatusCode < 400;

            _logger.LogInformation("API Call Successful => Route: {Controller}/{Action} - Status Code: {StatusCode}",
                sControllerName, sActionName, context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            logEntry.IsSuccess = false;
            logEntry.ErrorLog = ex.Message.ToString();

            _logger.LogError("API Call Failed => Route: {Controller}/{Action} - Status Code: 500 - Exception: {Message}", sControllerName, sActionName, ex.Message);
            throw;
        }
        finally
        {
            await db.SaveChangesAsync();
        }
    }
}
