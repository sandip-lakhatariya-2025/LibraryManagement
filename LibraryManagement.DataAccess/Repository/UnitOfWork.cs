using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.IRepository;
using LibraryManagement.DataAccess.Repository.IRepository;

namespace LibraryManagement.DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Backing fields for repositories
    private IBookRepository? _books;
    private IUserRepository? _users;
    private ICustomerRepository? _customers;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lazy-loaded repositories
    public IBookRepository Books =>
        _books ??= new BookRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public ICustomerRepository Customers =>
        _customers ??= new CustomerRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
