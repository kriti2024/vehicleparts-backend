using VehicleParts.Application.DTOs.Purchase;

namespace VehicleParts.Application.Interfaces;

public interface IPurchaseService
{
    Task<IEnumerable<PurchaseInvoiceDto>> GetAllPurchaseInvoicesAsync();
    Task<PurchaseInvoiceDto?> GetPurchaseInvoiceByIdAsync(int id);
    Task<PurchaseInvoiceDto> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceDto createDto);
    Task<bool> DeletePurchaseInvoiceAsync(int id);
}
