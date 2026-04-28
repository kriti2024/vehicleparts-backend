using VehicleParts.Application.DTOs.Part;

namespace VehicleParts.Application.Interfaces;

public interface IPartService
{
    Task<IEnumerable<PartDto>> GetAllPartsAsync();
    Task<PartDto?> GetPartByIdAsync(int id);
    Task<PartDto> CreatePartAsync(CreatePartDto createPartDto);
    Task<bool> UpdatePartAsync(UpdatePartDto updatePartDto);
    Task<bool> DeletePartAsync(int id);
    Task<IEnumerable<PartDto>> GetLowStockPartsAsync(int threshold = 10);
}
