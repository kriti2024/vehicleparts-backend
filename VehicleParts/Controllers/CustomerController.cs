using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.DTOs.Vehicle;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _context;

    public CustomerController(ICustomerService customerService, IEmailService emailService, IApplicationDbContext context)
    {
        _customerService = customerService;
        _emailService = emailService;
        _context = context;
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

    [HttpGet("search")]
    public async Task<ActionResult<List<CustomerSearchDTO>>> SearchCustomers([FromQuery] string keyword)
    {
        var result = await _customerService.SearchCustomersAsync(keyword);
        return Ok(result);
    }

    [HttpPost("send-reminders")]
    public async Task<IActionResult> SendCreditReminders()
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
        
        // Find customers with pending credit who haven't paid for more than 1 month
        var overdueCustomers = await _context.Customers
            .Where(c => c.PendingCredit > 0 && (!c.LastPaymentDate.HasValue || c.LastPaymentDate < oneMonthAgo))
            .ToListAsync();

        foreach (var customer in overdueCustomers)
        {
            if (!string.IsNullOrEmpty(customer.Email))
            {
                await _emailService.SendEmailAsync(customer.Email, "Payment Reminder: Overdue Credit", 
                    $"Dear {customer.FullName}, you have an outstanding balance of ${customer.PendingCredit} that is overdue by more than a month. Please settle your payment.");
            }
        }

        return Ok(new { message = $"Reminders sent to {overdueCustomers.Count} customers." });
    }
}