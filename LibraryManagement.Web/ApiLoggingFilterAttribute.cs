using LibraryManagement.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace LibraryManagement.Web;
public class ApiLoggingFilterAttribute : ActionFilterAttribute, IExceptionFilter
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var route = context.HttpContext.Request.Path;
        Logger.Log($"API Call Started: {route}");
        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var route = context.HttpContext.Request.Path;
        if (context.Exception == null)
        {
            Logger.Log($"API Call Successful: {route}");
        }
        base.OnActionExecuted(context);
    }

    public void OnException(ExceptionContext context)
    {
        var route = context.HttpContext.Request.Path;
        Logger.Log($"API Call Failed: {route} - Exception: {context.Exception.Message}");
    }
}
