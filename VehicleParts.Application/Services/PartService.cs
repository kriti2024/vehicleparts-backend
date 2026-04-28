using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Part;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Services;

public class PartService(IApplicationDbContext context) : IPartService
{
    public async Task<IEnumerable<PartDto>> GetAllPartsAsync()
    {
        return await context.Parts
            .Include(p => p.Vendor)
            .Select(p => new PartDto
            {
                PartId = p.PartId,
                PartName = p.PartName,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                VendorId = p.VendorId,
                VendorName = p.Vendor != null ? p.Vendor.VendorName : null
            })
            .ToListAsync();
    }

    public async Task<PartDto?> GetPartByIdAsync(int id)
    {
        var p = await context.Parts
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(x => x.PartId == id);

        if (p == null) return null;

        return new PartDto
        {
            PartId = p.PartId,
            PartName = p.PartName,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            VendorId = p.VendorId,
            VendorName = p.Vendor != null ? p.Vendor.VendorName : null
        };
    }

    public async Task<PartDto> CreatePartAsync(CreatePartDto dto)
    {
        var part = new Part
        {
            PartName = dto.PartName,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            VendorId = dto.VendorId
        };

        context.Parts.Add(part);
        await context.SaveChangesAsync();

        return await GetPartByIdAsync(part.PartId) ?? new PartDto();
    }

    public async Task<bool> UpdatePartAsync(UpdatePartDto dto)
    {
        var part = await context.Parts.FindAsync(dto.PartId);
        if (part == null) return false;

        part.PartName = dto.PartName;
        part.Price = dto.Price;
        part.StockQuantity = dto.StockQuantity;
        part.VendorId = dto.VendorId;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePartAsync(int id)
    {
        var part = await context.Parts.FindAsync(id);
        if (part == null) return false;

        context.Parts.Remove(part);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PartDto>> GetLowStockPartsAsync(int threshold = 10)
    {
        return await context.Parts
            .Include(p => p.Vendor)
            .Where(p => p.StockQuantity < threshold)
            .Select(p => new PartDto
            {
                PartId = p.PartId,
                PartName = p.PartName,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                VendorId = p.VendorId,
                VendorName = p.Vendor != null ? p.Vendor.VendorName : null
            })
            .ToListAsync();
    }
}
