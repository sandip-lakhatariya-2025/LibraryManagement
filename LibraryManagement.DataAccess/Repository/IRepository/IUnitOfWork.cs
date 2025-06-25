using LibraryManagement.DataAccess.IRepository;

namespace LibraryManagement.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    IBookRepository Books { get; }
    IUserRepository Users { get; }
    ICustomerRepository Customers { get; }
    Task<int> CompleteAsync();
}
