using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DataAccess.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerViewModel?> GetFirstOrDefaultSelectedAsync(long id)
    {
        return await _context.Customers.Where(c => c.Id == id).Select(c => new CustomerViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email
        }).FirstOrDefaultAsync();
    }

    public async Task<bool> InsertAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        return await _context.SaveChangesAsync() > 0;
    }

}
