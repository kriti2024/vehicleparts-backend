using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Part;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartController(IPartService partService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetAll()
    {
        IEnumerable<PartDto> parts;
        try
        {
            parts = await partService.GetAllPartsAsync();
            if (!parts.Any())
            {
                parts = StaffFallbackStore.GetParts();
            }
        }
        catch
        {
            parts = StaffFallbackStore.GetParts();
        }

        return Ok(parts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PartDto>> GetById(int id)
    {
        PartDto? part;
        try
        {
            part = await partService.GetPartByIdAsync(id);
        }
        catch
        {
            part = StaffFallbackStore.GetParts().FirstOrDefault(p => p.PartId == id);
        }

        part ??= StaffFallbackStore.GetParts().FirstOrDefault(p => p.PartId == id);
        if (part == null) return NotFound();
        return Ok(part);
    }

    [HttpPost]
    public async Task<ActionResult<PartDto>> Create(CreatePartDto dto)
    {
        var part = await partService.CreatePartAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = part.PartId }, part);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePartDto dto)
    {
        if (id != dto.PartId) return BadRequest();
        var success = await partService.UpdatePartAsync(dto);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await partService.DeletePartAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetLowStock([FromQuery] int threshold = 10)
    {
        IEnumerable<PartDto> parts;
        try
        {
            parts = await partService.GetLowStockPartsAsync(threshold);
            if (!parts.Any())
            {
                parts = StaffFallbackStore.GetParts().Where(p => p.StockQuantity < threshold);
            }
        }
        catch
        {
            parts = StaffFallbackStore.GetParts().Where(p => p.StockQuantity < threshold);
        }

        return Ok(parts);
    }
}
