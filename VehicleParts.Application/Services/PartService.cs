using Microsoft.AspNetCore.Http;
using System.IO;
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
                VendorName = p.Vendor != null ? p.Vendor.VendorName : null,
                ImageUrl = p.ImageUrl,
                VehicleBrand = p.VehicleBrand,
                VehicleModel = p.VehicleModel
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
            VendorName = p.Vendor != null ? p.Vendor.VendorName : null,
            ImageUrl = p.ImageUrl,
            VehicleBrand = p.VehicleBrand,
            VehicleModel = p.VehicleModel
        };
    }

    public async Task<PartDto> CreatePartAsync(CreatePartDto dto)
    {
        string? imagePath = null;

        if (dto.ImageFile != null)
        {
            var uploadsFolder =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/parts"
                );

            Directory.CreateDirectory(uploadsFolder);

            var fileName =
                Guid.NewGuid() +
                Path.GetExtension(dto.ImageFile.FileName);

            var filePath =
                Path.Combine(
                    uploadsFolder,
                    fileName
                );

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create
                );

            await dto.ImageFile.CopyToAsync(stream);

            imagePath =
                $"/uploads/parts/{fileName}";
        }

        var part = new Part
        {
            PartName = dto.PartName,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            VendorId = dto.VendorId,
            ImageUrl = imagePath,
            VehicleBrand = dto.VehicleBrand,
            VehicleModel = dto.VehicleModel
        };

        context.Parts.Add(part);

        await context.SaveChangesAsync();

        return await GetPartByIdAsync(part.PartId)
               ?? new PartDto();
    }
    public async Task<bool> UpdatePartAsync(UpdatePartDto dto)
    {
        var part =
            await context.Parts.FindAsync(dto.PartId);

        if (part == null)
            return false;

        part.PartName = dto.PartName;
        part.Price = dto.Price;
        part.StockQuantity = dto.StockQuantity;
        part.VendorId = dto.VendorId;
        part.VehicleBrand = dto.VehicleBrand;
        part.VehicleModel = dto.VehicleModel;

        if (dto.ImageFile != null)
        {
            var uploadsFolder =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/parts"
                );

            Directory.CreateDirectory(uploadsFolder);

            var fileName =
                Guid.NewGuid() +
                Path.GetExtension(dto.ImageFile.FileName);

            var filePath =
                Path.Combine(
                    uploadsFolder,
                    fileName
                );

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create
                );

            await dto.ImageFile.CopyToAsync(stream);

            part.ImageUrl =
                $"/uploads/parts/{fileName}";
        }

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
                VendorName = p.Vendor != null ? p.Vendor.VendorName : null,
                ImageUrl = p.ImageUrl,
                VehicleBrand = p.VehicleBrand,
                VehicleModel = p.VehicleModel
            })
            .ToListAsync();
    }
}
