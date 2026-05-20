using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Vendor;
using VehicleParts.Application.Exceptions;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Services;

public class VendorService(IApplicationDbContext context) : IVendorService
{
    public async Task<IEnumerable<VendorDto>> GetAllVendorsAsync()
    {
        return await context.Vendors.Where(v => v.IsActive)
            .Select(v => new VendorDto
            {
                VendorId = v.VendorId,
                VendorName = v.VendorName,
                Phone = v.Phone,
                Address = v.Address
            })
            .ToListAsync();
    }

    public async Task<VendorDto?> GetVendorByIdAsync(int id)
    {
        var v = await context.Vendors.FindAsync(id);
        if (v == null) return null;

        return new VendorDto
        {
            VendorId = v.VendorId,
            VendorName = v.VendorName,
            Phone = v.Phone,
            Address = v.Address
        };
    }

    public async Task<VendorDto> CreateVendorAsync(CreateVendorDto dto)
    {
        var vendor = new Vendor
        {
            VendorName = dto.VendorName,
            Phone = dto.Phone,
            Address = dto.Address
        };

        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();

        return new VendorDto
        {
            VendorId = vendor.VendorId,
            VendorName = vendor.VendorName,
            Phone = vendor.Phone,
            Address = vendor.Address
        };
    }

    public async Task<bool> UpdateVendorAsync(UpdateVendorDto dto)
    {
        var vendor = await context.Vendors.FindAsync(dto.VendorId);
        if (vendor == null) return false;

        vendor.VendorName = dto.VendorName;
        vendor.Phone = dto.Phone;
        vendor.Address = dto.Address;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVendorAsync(int id)
    {
        var vendor = await context.Vendors.FindAsync(id);

        if (vendor == null)
            return false;

        vendor.IsActive = false;

        await context.SaveChangesAsync();

        return true;
    }
}
