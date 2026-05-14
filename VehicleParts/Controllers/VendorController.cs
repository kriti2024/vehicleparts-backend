using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Infrastructure.Data;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorController : ControllerBase
{
    private readonly AppDbContext _context;

    public VendorController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vendors = await _context.Vendors
            .Select(vendor => new
            {
                vendor.VendorId,
                vendor.VendorName,
                Email = "",
                PhoneNumber = vendor.Phone,
                vendor.Address,
                TotalParts = _context.Parts.Count(part => part.VendorId == vendor.VendorId)
            })
            .ToListAsync();

        return Ok(vendors);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vendor = await _context.Vendors
            .Where(item => item.VendorId == id)
            .Select(item => new
            {
                item.VendorId,
                item.VendorName,
                Email = "",
                PhoneNumber = item.Phone,
                item.Address
            })
            .FirstOrDefaultAsync();

        return vendor == null ? NotFound() : Ok(vendor);
    }

    [HttpPost]
    public async Task<IActionResult> Create(VendorDto dto)
    {
        var vendor = new Vendor
        {
            VendorName = dto.VendorName.Trim(),
            Phone = dto.PhoneNumber,
            Address = dto.Address
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = vendor.VendorId }, vendor);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, VendorDto dto)
    {
        var vendor = await _context.Vendors.FindAsync(id);

        if (vendor == null)
            return NotFound();

        vendor.VendorName = dto.VendorName.Trim();
        vendor.Phone = dto.PhoneNumber;
        vendor.Address = dto.Address;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);

        if (vendor == null)
            return NotFound();

        var hasParts = await _context.Parts.AnyAsync(part => part.VendorId == id);
        if (hasParts)
            return BadRequest(new { message = "Vendor has parts assigned and cannot be deleted." });

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class VendorDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
