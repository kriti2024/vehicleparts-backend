using VehicleParts.Application.DTOs.Reports;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<FinancialReportDTO> GetFinancialSummaryAsync()
    {
        return await _reportRepository.GetFinancialSummaryAsync();
    }

    public async Task<CustomerReportDTO> GetCustomerReportAsync()
    {
        return await _reportRepository.GetCustomerReportAsync();
    }
}