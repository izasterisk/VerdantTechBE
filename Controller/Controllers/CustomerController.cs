using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Customer;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    
    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Tạo khách hàng mới
    /// </summary>
    /// <param name="dto">Thông tin khách hàng cần tạo</param>
    /// <returns>Thông tin khách hàng đã tạo</returns>
    [HttpPost]
    public async Task<ActionResult<APIResponse>> CreateCustomer([FromBody] CustomerCreateDTO dto)
    {
        var response = new APIResponse();
        
        try
        {
            if (!ModelState.IsValid)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var customer = await _customerService.CreateCustomerAsync(dto);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.Created;
            response.Data = customer;
            
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, response);
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors.Add(ex.Message);
            return BadRequest(response);
        }
    }

    /// <summary>
    /// Lấy thông tin khách hàng theo ID
    /// </summary>
    /// <param name="id">ID của khách hàng</param>
    /// <returns>Thông tin khách hàng</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<APIResponse>> GetCustomerById(ulong id)
    {
        var response = new APIResponse();
        
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            
            if (customer == null)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add($"Không tìm thấy khách hàng với ID {id}");
                return NotFound(response);
            }

            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = customer;
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Errors.Add(ex.Message);
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả khách hàng với phân trang
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách khách hàng có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<APIResponse>> GetAllCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = new APIResponse();
        
        try
        {
            // Validate pagination parameters
            if (page < 1)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Page number must be greater than 0");
                return BadRequest(response);
            }

            if (pageSize < 1 || pageSize > 100)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Page size must be between 1 and 100");
                return BadRequest(response);
            }

            var customers = await _customerService.GetAllCustomersAsync(page, pageSize);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = customers;
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Errors.Add(ex.Message);
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Cập nhật thông tin khách hàng
    /// </summary>
    /// <param name="id">ID của khách hàng</param>
    /// <param name="dto">Thông tin khách hàng cần cập nhật</param>
    /// <returns>Thông tin khách hàng đã cập nhật</returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<APIResponse>> UpdateCustomer(ulong id, [FromBody] CustomerUpdateDTO dto)
    {
        var response = new APIResponse();
        
        try
        {
            if (!ModelState.IsValid)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var customer = await _customerService.UpdateCustomerAsync(id, dto);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = customer;
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors.Add(ex.Message);
            return BadRequest(response);
        }
    }
}