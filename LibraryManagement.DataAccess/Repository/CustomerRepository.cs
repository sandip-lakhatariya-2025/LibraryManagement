using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DataAccess.Repository;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

}
