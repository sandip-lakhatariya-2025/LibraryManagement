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
    public async Task<Response<CustomerViewModel?>> AddCustomer(CustomerViewModel customerViewModel)
    {

        Customer newCustomer = new Customer
        {
            Name = customerViewModel.Name,
            Email = customerViewModel.Email
        };

        bool isSuccess = await _customerRepository.InsertAsync(newCustomer);
        
        if (isSuccess)
        {
            CustomerViewModel? addedCustomer = await GetCustomerById(newCustomer.Id);
            return CommonHelper.CreateResponse(addedCustomer, HttpStatusCode.OK, true, "Customer added successfully.");
        }

        return CommonHelper.CreateResponse<CustomerViewModel?>(null, HttpStatusCode.BadRequest, false, $"Failed to add Customer.");
    }

    public async Task<CustomerViewModel?> GetCustomerById(long id)
    {
        return await _customerRepository.GetFirstOrDefaultSelectedAsync(id);
    }
}
