using System.Net;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services.IServices;

namespace LibraryManagement.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    public async Task<Response<CustomerViewModel?>> AddCustomer(CustomerViewModel objCustomerViewModel)
    {

        Customer objNewCustomer = new Customer
        {
            Name = objCustomerViewModel.Name,
            Email = objCustomerViewModel.Email
        };

        await _customerRepository.InsertAsync(objNewCustomer);
        await _customerRepository.SaveChangesAsync();

        CustomerViewModel? objAddedCustomer = await GetCustomerById(objNewCustomer.Id);

        return CommonHelper.CreateResponse(objAddedCustomer, HttpStatusCode.OK, true, "Customer added successfully.");

    }

    public async Task<CustomerViewModel?> GetCustomerById(long id)
    {
        return await _customerRepository.GetFirstOrDefaultAsync(
            c => c.Id == id,
            c => new CustomerViewModel {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            }
        );
    }
}
