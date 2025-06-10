using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;

namespace LibraryManagement.DataAccess.Repository.IRepository;

public interface ICustomerRepository
{
    Task<CustomerViewModel?> GetFirstOrDefaultSelectedAsync(long id);
    Task<bool> InsertAsync(Customer customer);
}
