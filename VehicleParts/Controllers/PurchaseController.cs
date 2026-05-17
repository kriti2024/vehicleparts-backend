using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Purchase;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController(IPurchaseService purchaseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseInvoiceDto>>> GetAll()
    {
        var invoices = await purchaseService.GetAllPurchaseInvoicesAsync();
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseInvoiceDto>> GetById(int id)
    {
        var invoice = await purchaseService.GetPurchaseInvoiceByIdAsync(id);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseInvoiceDto>> Create(CreatePurchaseInvoiceDto dto)
    {
        var invoice = await purchaseService.CreatePurchaseInvoiceAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = invoice.PurchaseInvoiceId }, invoice);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await purchaseService.DeletePurchaseInvoiceAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
