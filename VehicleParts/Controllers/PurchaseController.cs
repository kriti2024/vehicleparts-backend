using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Domain.Entities;
using VehicleParts.Infrastructure.Data;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController : ControllerBase
{
    private readonly AppDbContext _context;

    public PurchaseController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var invoices = await _context.PurchaseInvoices
            .Include(invoice => invoice.Vendor)
            .Include(invoice => invoice.Items)
            .ThenInclude(item => item.Part)
            .OrderByDescending(invoice => invoice.PurchaseDate)
            .Select(invoice => new
            {
                invoice.PurchaseInvoiceId,
                VendorName = invoice.Vendor != null ? invoice.Vendor.VendorName : "",
                invoice.PurchaseDate,
                invoice.TotalAmount,
                Items = invoice.Items.Select(item => new
                {
                    item.PurchaseInvoiceItemId,
                    PartName = item.Part != null ? item.Part.PartName : "",
                    item.Quantity,
                    item.UnitPrice,
                    item.TotalPrice
                })
            })
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _context.PurchaseInvoices
            .Include(item => item.Items)
            .ThenInclude(item => item.Part)
            .Include(item => item.Vendor)
            .FirstOrDefaultAsync(item => item.PurchaseInvoiceId == id);

        return invoice == null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseInvoiceDto dto)
    {
        if (!await _context.Vendors.AnyAsync(vendor => vendor.VendorId == dto.VendorId))
            return BadRequest(new { message = "Vendor does not exist." });

        var invoice = new PurchaseInvoice
        {
            VendorId = dto.VendorId,
            PurchaseDate = dto.PurchaseDate,
            Items = dto.Items.Select(item => new PurchaseInvoiceItem
            {
                PartId = item.PartId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.Quantity * item.UnitPrice
            }).ToList()
        };

        invoice.TotalAmount = invoice.Items.Sum(item => item.TotalPrice);

        foreach (var item in invoice.Items)
        {
            var part = await _context.Parts.FindAsync(item.PartId);
            if (part == null)
                return BadRequest(new { message = $"Part {item.PartId} does not exist." });

            part.StockQuantity += item.Quantity;
        }

        _context.PurchaseInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.PurchaseInvoiceId },
            new
            {
                invoice.PurchaseInvoiceId,
                invoice.VendorId,
                invoice.PurchaseDate,
                invoice.TotalAmount,
                Items = invoice.Items.Select(item => new
                {
                    item.PurchaseInvoiceItemId,
                    item.PartId,
                    item.Quantity,
                    item.UnitPrice,
                    item.TotalPrice
                })
            });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _context.PurchaseInvoices
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.PurchaseInvoiceId == id);

        if (invoice == null)
            return NotFound();

        foreach (var item in invoice.Items)
        {
            var part = await _context.Parts.FindAsync(item.PartId);
            if (part != null)
            {
                part.StockQuantity = Math.Max(0, part.StockQuantity - item.Quantity);
            }
        }

        _context.PurchaseInvoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreatePurchaseInvoiceDto
{
    public int VendorId { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public List<CreatePurchaseInvoiceItemDto> Items { get; set; } = new();
}

public class CreatePurchaseInvoiceItemDto
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
