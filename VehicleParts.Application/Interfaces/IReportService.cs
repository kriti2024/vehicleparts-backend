using VehicleParts.Application.DTOs.Reports;

namespace VehicleParts.Application.Interfaces;

public interface IReportService
{
    Task<FinancialReportDTO> GetFinancialSummaryAsync();
    Task<SimpleFinancialReportDTO> GetFinancialReportAsync(string period);
    Task<List<MonthlyRevenueDTO>> GetMonthlyRevenueAsync();
    Task<CustomerReportDTO> GetCustomerReportAsync();
}
