using VehicleParts.Application.DTOs.Sale;

namespace VehicleParts.Application.Interfaces;

public interface ISalesService
{
    Task<SaleDTO> CreateSaleAsync(CreateSaleDTO dto);
    Task<SaleDTO?> GetSaleByIdAsync(int saleId);
    Task<InvoiceDTO?> GetInvoiceAsync(int saleId);
    Task<List<SaleDTO>> GetCustomerSalesAsync(int customerId);
    Task SendInvoiceEmailAsync(int saleId);
}