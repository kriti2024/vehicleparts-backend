using VehicleParts.Application.DTOs.Vendor;

namespace VehicleParts.Application.Interfaces;

public interface IVendorService
{
    Task<IEnumerable<VendorDto>> GetAllVendorsAsync();
    Task<VendorDto?> GetVendorByIdAsync(int id);
    Task<VendorDto> CreateVendorAsync(CreateVendorDto createVendorDto);
    Task<bool> UpdateVendorAsync(UpdateVendorDto updateVendorDto);
    Task<bool> DeleteVendorAsync(int id);
}
