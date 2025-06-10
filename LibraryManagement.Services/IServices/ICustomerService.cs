using LibraryManagement.Common;
using LibraryManagement.Models.ViewModels;

namespace LibraryManagement.Services.IServices;

public interface ICustomerService
{
    Task<CustomerViewModel?> GetCustomerById(long id);
    Task<Response<CustomerViewModel?>> AddCustomer(CustomerViewModel customerViewModel);
}
