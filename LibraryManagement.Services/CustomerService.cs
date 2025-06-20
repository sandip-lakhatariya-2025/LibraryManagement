using System.Net;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;
using LibraryManagement.Services.IServices;

namespace LibraryManagement.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    public async Task<Response<CustomerResultDto?>> AddCustomer(CustomerCreateDto objCustomerCreateDto)
    {

        Customer objNewCustomer = new Customer
        {
            Name = objCustomerCreateDto.Name,
            Email = objCustomerCreateDto.Email
        };

        await _customerRepository.InsertAsync(objNewCustomer);
        await _customerRepository.SaveChangesAsync();

        CustomerResultDto? objAddedCustomer = await GetCustomerById(objNewCustomer.Id);

        return CommonHelper.CreateResponse(objAddedCustomer, HttpStatusCode.OK, true, "Customer added successfully.");

    }

    public async Task<CustomerResultDto?> GetCustomerById(long id)
    {
        return await _customerRepository.GetFirstOrDefaultAsync(
            c => c.Id == id,
            c => new CustomerResultDto {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            }
        );
    }
}
