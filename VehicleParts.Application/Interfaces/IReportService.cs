using VehicleParts.Application.DTOs.Reports;

namespace VehicleParts.Application.Interfaces;

public interface IReportService
{
    Task<FinancialReportDTO> GetFinancialSummaryAsync();
    Task<CustomerReportDTO> GetCustomerReportAsync();
}