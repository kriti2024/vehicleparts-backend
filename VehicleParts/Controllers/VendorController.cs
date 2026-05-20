using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Vendor;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorController(IVendorService vendorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetAll()
    {
        var vendors = await vendorService.GetAllVendorsAsync();
        return Ok(vendors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VendorDto>> GetById(int id)
    {
        var vendor = await vendorService.GetVendorByIdAsync(id);
        if (vendor == null) return NotFound();
        return Ok(vendor);
    }

    [HttpPost]
    public async Task<ActionResult<VendorDto>> Create(CreateVendorDto dto)
    {
        var vendor = await vendorService.CreateVendorAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = vendor.VendorId }, vendor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateVendorDto dto)
    {
        if (id != dto.VendorId)
        {
            return BadRequest(new
            {
                message = "Vendor ID mismatch."
            });
        }
        var success = await vendorService.UpdateVendorAsync(dto);
        if (!success)
        {
            return NotFound(new
            {
                message = "Vendor not found"
            });
        }
        return Ok(new
        {
            message = "Vendor updated successfully."
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await vendorService.DeleteVendorAsync(id);

        if (!success)
        {
            return NotFound(new
            {
                message = "Vendor not found."
            });
        }

        return Ok(new
        {
            message = "Vendor deleted successfully."
        });
    }
}
