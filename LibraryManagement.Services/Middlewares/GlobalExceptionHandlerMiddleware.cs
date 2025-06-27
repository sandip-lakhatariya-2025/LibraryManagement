using System.Net;
using LibraryManagement.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = CommonHelper.CreateResponse(
            false, 
            HttpStatusCode.NotFound, 
            false, 
            "An unexpected error occurred.",
            new string[] { exception.Message }
        );

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(jsonResponse);
    }
}
