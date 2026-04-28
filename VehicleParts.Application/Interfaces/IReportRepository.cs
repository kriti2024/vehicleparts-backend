using VehicleParts.Application.DTOs.Reports;

namespace VehicleParts.Application.Interfaces;

public interface IReportRepository
{
    Task<FinancialReportDTO> GetFinancialSummaryAsync();
    Task<CustomerReportDTO> GetCustomerReportAsync();
}