using LibraryManagement.DataAccess.IRepository;
using LibraryManagement.DataAccess.Repository;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Services;
using LibraryManagement.Services.IServices;

namespace LibraryManagement.Web.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services) {

        #region repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        #endregion

        #region services
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAuthService, AuthService>();

        #endregion

        return services;
    }
}
