using LibraryManagement.Common;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;

namespace LibraryManagement.Services.IServices;

public interface ICustomerService
{
    Task<CustomerResultDto?> GetCustomerById(long id);
    Task<Response<CustomerResultDto?>> AddCustomer(CustomerCreateDto objCustomerCreateDto);
}
