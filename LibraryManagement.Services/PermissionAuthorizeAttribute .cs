namespace LibraryManagement.Services;
using LibraryManagement.DataAccess.Data;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PermissionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly ClientEndpoint _clientEndpoint;
    private readonly Permission _permission;

    public PermissionAuthorizeAttribute(ClientEndpoint clientEndpoint, Permission permission) {
        _clientEndpoint = clientEndpoint;
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context) {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var endPointPermission = await dbContext.EndPointPermissions.FirstOrDefaultAsync(p => p.EndpointName == _clientEndpoint.ToString());

        if(endPointPermission == null || !IsPermissionGranted(endPointPermission, _permission)) {
            context.Result = new UnauthorizedResult();
        }
    }

    private bool IsPermissionGranted(EndPointPermission endPointPermission, Permission permission) {
        return permission switch {
            Permission.Read => endPointPermission.CanRead,
            Permission.Write => endPointPermission.CanWrite,
            Permission.Update => endPointPermission.CanUpdate,
            Permission.Delete => endPointPermission.CanDelete,
            Permission.MultipleUpdate => endPointPermission.CanMultipleUpdate,
            Permission.MultipleDelete => endPointPermission.CanMultipleDelete,
            _ => false
        };
    }
}
