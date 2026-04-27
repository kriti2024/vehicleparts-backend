using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Reports;
using VehicleParts.Application.Interfaces;
using VehicleParts.Infrastructure.Data;

namespace VehicleParts.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialReportDTO> GetFinancialSummaryAsync()
    {
        var today = DateTime.UtcNow.Date;
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        var dailySales = await _context.Sales
            .Where(x => x.SaleDate.Date == today)
            .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;

        var monthlySales = await _context.Sales
            .Where(x => x.SaleDate.Month == month && x.SaleDate.Year == year)
            .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;

        var yearlySales = await _context.Sales
            .Where(x => x.SaleDate.Year == year)
            .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;

        var dailyInvoices = await _context.Sales
            .CountAsync(x => x.SaleDate.Date == today);

        var monthlyInvoices = await _context.Sales
            .CountAsync(x => x.SaleDate.Month == month && x.SaleDate.Year == year);

        var yearlyInvoices = await _context.Sales
            .CountAsync(x => x.SaleDate.Year == year);

        return new FinancialReportDTO
        {
            DailySales = dailySales,
            MonthlySales = monthlySales,
            YearlySales = yearlySales,

            DailyInvoices = dailyInvoices,
            MonthlyInvoices = monthlyInvoices,
            YearlyInvoices = yearlyInvoices
        };
    }
}