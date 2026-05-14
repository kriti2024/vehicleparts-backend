using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Controllers;

public class PartFormDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int VendorId { get; set; }
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
    public IFormFile? ImageFile { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class PartController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public PartController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var parts = await _context.Parts
            .Include(part => part.Vendor)
            .Select(part => new
            {
                part.PartId,
                part.PartName,
                part.Price,
                part.StockQuantity,
                part.VendorId,
                VendorName = part.Vendor != null ? part.Vendor.VendorName : null
            })
            .ToListAsync();

        return Ok(parts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var part = await _context.Parts
            .Include(item => item.Vendor)
            .Where(item => item.PartId == id)
            .Select(item => new
            {
                item.PartId,
                item.PartName,
                item.Price,
                item.StockQuantity,
                item.VendorId,
                VendorName = item.Vendor != null ? item.Vendor.VendorName : null,
                VehicleBrand = "",
                VehicleModel = "",
                ImageUrl = ""
            })
            .FirstOrDefaultAsync();

        return part == null ? NotFound() : Ok(part);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] PartFormDto dto)
    {
        if (!await _context.Vendors.AnyAsync(vendor => vendor.VendorId == dto.VendorId))
            return BadRequest(new { message = "Vendor does not exist." });

        var part = new Part
        {
            PartName = dto.PartName.Trim(),
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            VendorId = dto.VendorId
        };

        _context.Parts.Add(part);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = part.PartId }, part);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] PartFormDto dto)
    {
        var part = await _context.Parts.FindAsync(id);

        if (part == null)
            return NotFound();

        if (!await _context.Vendors.AnyAsync(vendor => vendor.VendorId == dto.VendorId))
            return BadRequest(new { message = "Vendor does not exist." });

        part.PartName = dto.PartName.Trim();
        part.Price = dto.Price;
        part.StockQuantity = dto.StockQuantity;
        part.VendorId = dto.VendorId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var part = await _context.Parts.FindAsync(id);

        if (part == null)
            return NotFound();

        var notifications = await _context.Notifications
            .Where(notification => notification.PartId == id)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.PartId = null;
        }

        _context.Parts.Remove(part);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
    {
        var parts = await _context.Parts
            .Include(part => part.Vendor)
            .Where(part => part.StockQuantity < threshold)
            .OrderBy(part => part.StockQuantity)
            .Select(part => new
            {
                part.PartId,
                part.PartName,
                part.Price,
                part.StockQuantity,
                part.VendorId,
                VendorName = part.Vendor != null ? part.Vendor.VendorName : null
            })
            .ToListAsync();

        return Ok(parts);
    }
}
