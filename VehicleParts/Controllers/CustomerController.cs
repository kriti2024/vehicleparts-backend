using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.DTOs.Vehicle;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // POST: api/customer
    [HttpPost]
    public async Task<ActionResult<CustomerDTO>> CreateCustomer([FromBody] CreateCustomerDTO dto)
    {
        try
        {
            var customer = await _customerService.CreateCustomerAsync(dto);
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
        }
        catch
        {
            var customer = StaffFallbackStore.CreateCustomer(dto);
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
        }
    }

    // GET: api/customer/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDTO>> GetCustomerById(int id)
    {
        CustomerDTO? customer;
        try
        {
            customer = await _customerService.GetCustomerByIdAsync(id);
        }
        catch
        {
            customer = StaffFallbackStore.GetCustomer(id);
        }

        if (customer == null)
            return NotFound(new { message = $"Customer with ID {id} not found" });

        return Ok(customer);
    }

    // GET: api/customer/{id}/with-vehicles
    [HttpGet("{id}/with-vehicles")]
    public async Task<ActionResult<CustomerWithVehiclesDTO>> GetCustomerWithVehicles(int id)
    {
        CustomerWithVehiclesDTO? customer;
        try
        {
            customer = await _customerService.GetCustomerWithVehiclesAsync(id);
        }
        catch
        {
            customer = StaffFallbackStore.GetCustomerWithVehicles(id);
        }

        if (customer == null)
            return NotFound(new { message = $"Customer with ID {id} not found" });

        return Ok(customer);
    }

    // GET: api/customer
    [HttpGet]
    public async Task<ActionResult<List<CustomerDTO>>> GetAllCustomers()
    {
        List<CustomerDTO> customers;
        try
        {
            customers = await _customerService.GetAllCustomersAsync();
        }
        catch
        {
            customers = StaffFallbackStore.GetCustomers();
        }

        return Ok(customers);
    }

    // POST: api/customer/vehicle
    [HttpPost("vehicle")]
    public async Task<ActionResult<VehicleDTO>> AddVehicle([FromBody] CreateVehicleDTO dto)
    {
        try
        {
            var vehicle = await _customerService.AddVehicleAsync(dto);
            return Ok(vehicle);
        }
        catch
        {
            try
            {
                return Ok(StaffFallbackStore.AddVehicle(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // GET: api/customer/{id}/vehicles
    [HttpGet("{id}/vehicles")]
    public async Task<ActionResult<List<VehicleDTO>>> GetCustomerVehicles(int id)
    {
        List<VehicleDTO> vehicles;
        try
        {
            vehicles = await _customerService.GetCustomerVehiclesAsync(id);
        }
        catch
        {
            vehicles = StaffFallbackStore.GetCustomerVehicles(id);
        }

        return Ok(vehicles);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<CustomerSearchDTO>>> SearchCustomers([FromQuery] string keyword)
    {
        List<CustomerSearchDTO> result;
        try
        {
            result = await _customerService.SearchCustomersAsync(keyword);
        }
        catch
        {
            result = StaffFallbackStore.SearchCustomers(keyword);
        }

        return Ok(result);
    }
}
