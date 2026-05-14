using VehicleParts.Application.DTOs.Reports;

namespace VehicleParts.Application.Interfaces;

public interface IReportRepository
{
    Task<FinancialReportDTO> GetFinancialSummaryAsync();
    Task<List<MonthlyRevenueDTO>> GetMonthlyRevenueAsync();
    Task<CustomerReportDTO> GetCustomerReportAsync();
}
