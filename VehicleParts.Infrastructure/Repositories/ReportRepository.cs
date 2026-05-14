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

    public async Task<CustomerReportDTO> GetCustomerReportAsync()
    {
        // 1. Regular Customers (Top 10 by sale count)
        var regulars = await _context.Customers
            .Select(c => new CustomerSummaryDTO
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Email = c.Email ?? string.Empty,
                Phone = c.Phone ?? string.Empty,
                TotalPurchases = _context.Sales.Count(s => s.CustomerId == c.CustomerId),
                TotalSpent = _context.Sales.Where(s => s.CustomerId == c.CustomerId).Sum(s => (decimal?)s.FinalAmount) ?? 0
            })
            .Where(c => c.TotalPurchases > 0)
            .OrderByDescending(c => c.TotalPurchases)
            .Take(10)
            .ToListAsync();

        // 2. High Spenders (Top 10 by total spent)
        var highSpenders = await _context.Customers
            .Select(c => new CustomerSummaryDTO
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Email = c.Email ?? string.Empty,
                Phone = c.Phone ?? string.Empty,
                TotalPurchases = _context.Sales.Count(s => s.CustomerId == c.CustomerId),
                TotalSpent = _context.Sales.Where(s => s.CustomerId == c.CustomerId).Sum(s => (decimal?)s.FinalAmount) ?? 0
            })
            .Where(c => c.TotalSpent > 0)
            .OrderByDescending(c => c.TotalSpent)
            .Take(10)
            .ToListAsync();

        // 3. Pending Credit Customers (Customers with Pending or Credit status)
        var pendingCredits = await _context.Sales
            .Where(s => s.PaymentStatus == Domain.Enums.PaymentStatus.Pending || s.PaymentStatus == Domain.Enums.PaymentStatus.Credit)
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                PendingAmount = g.Sum(s => s.FinalAmount)
            })
            .Join(_context.Customers, 
                g => g.CustomerId, 
                c => c.CustomerId, 
                (g, c) => new CustomerSummaryDTO
                {
                    CustomerId = c.CustomerId,
                    FullName = c.FullName,
                    Email = c.Email ?? string.Empty,
                    Phone = c.Phone ?? string.Empty,
                    PendingAmount = g.PendingAmount
                })
            .ToListAsync();

        return new CustomerReportDTO
        {
            RegularCustomers = regulars,
            HighSpenders = highSpenders,
            PendingCreditCustomers = pendingCredits
        };
    }
}
