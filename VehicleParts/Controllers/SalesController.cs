using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Sale;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    // POST: api/sales
    [HttpPost]
    public async Task<ActionResult<SaleDTO>> CreateSale([FromBody] CreateSaleDTO dto)
    {
        try
        {
            var sale = await _salesService.CreateSaleAsync(dto);
            return CreatedAtAction(nameof(GetSaleById), new { id = sale.SaleId }, sale);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/sales/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDTO>> GetSaleById(int id)
    {
        var sale = await _salesService.GetSaleByIdAsync(id);

        if (sale == null)
            return NotFound(new { message = $"Sale with ID {id} not found" });

        return Ok(sale);
    }

    // GET: api/sales/{id}/invoice
    [HttpGet("{id}/invoice")]
    public async Task<ActionResult<InvoiceDTO>> GetInvoice(int id)
    {
        var invoice = await _salesService.GetInvoiceAsync(id);

        if (invoice == null)
            return NotFound(new { message = $"Sale with ID {id} not found" });

        return Ok(invoice);
    }

    // GET: api/sales/customer/{customerId}
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<SaleDTO>>> GetCustomerSales(int customerId)
    {
        var sales = await _salesService.GetCustomerSalesAsync(customerId);
        return Ok(sales);
    }
}