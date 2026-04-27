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
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/customer/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDTO>> GetCustomerById(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);

        if (customer == null)
            return NotFound(new { message = $"Customer with ID {id} not found" });

        return Ok(customer);
    }

    // GET: api/customer/{id}/with-vehicles
    [HttpGet("{id}/with-vehicles")]
    public async Task<ActionResult<CustomerWithVehiclesDTO>> GetCustomerWithVehicles(int id)
    {
        var customer = await _customerService.GetCustomerWithVehiclesAsync(id);

        if (customer == null)
            return NotFound(new { message = $"Customer with ID {id} not found" });

        return Ok(customer);
    }

    // GET: api/customer
    [HttpGet]
    public async Task<ActionResult<List<CustomerDTO>>> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
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
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/customer/{id}/vehicles
    [HttpGet("{id}/vehicles")]
    public async Task<ActionResult<List<VehicleDTO>>> GetCustomerVehicles(int id)
    {
        var vehicles = await _customerService.GetCustomerVehiclesAsync(id);
        return Ok(vehicles);
    }
}