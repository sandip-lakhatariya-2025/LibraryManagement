using System.Net;
using AutoMapper;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Dtos.RequestDtos;
using LibraryManagement.Models.Dtos.ResponseDtos;
using LibraryManagement.Models.Models;
using LibraryManagement.Services.IServices;

namespace LibraryManagement.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<Response<CustomerResultDto?>> AddCustomer(CustomerCreateDto objCustomerCreateDto)
    {
        Customer objNewCustomer = _mapper.Map<Customer>(objCustomerCreateDto);

        await _unitOfWork.Customers.InsertAsync(objNewCustomer);
        await  _unitOfWork.CompleteAsync();

        CustomerResultDto? objAddedCustomer = await GetCustomerById(objNewCustomer.Id);

        return CommonHelper.CreateResponse(objAddedCustomer, HttpStatusCode.OK, true, "Customer added successfully.");
    }

    public async Task<CustomerResultDto?> GetCustomerById(long id)
    {
        return await _unitOfWork.Customers.GetFirstOrDefaultAsync<CustomerResultDto>(
            c => c.Id == id,
            _mapper.ConfigurationProvider
        );
    }
}
