using System.Globalization;
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

    public async Task<SimpleFinancialReportDTO> GetFinancialReportAsync(string period)
    {
        var (normalizedPeriod, periodLabel, startDate, endDate) =
            GetReportPeriod(period);

        var salesQuery = _context.Sales
            .AsNoTracking()
            .Where(x => x.SaleDate >= startDate && x.SaleDate < endDate);

        var purchasesQuery = _context.PurchaseInvoices
            .AsNoTracking()
            .Where(x => x.PurchaseDate >= startDate && x.PurchaseDate < endDate);

        var totalSalesRevenue = await salesQuery
            .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;

        var totalPurchaseCost = await purchasesQuery
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

        var totalDiscountGiven = await salesQuery
            .SumAsync(x => (decimal?)x.DiscountAmount) ?? 0;

        var salesInvoiceCount = await salesQuery.CountAsync();
        var purchaseInvoiceCount = await purchasesQuery.CountAsync();

        var recentSales = await salesQuery
            .OrderByDescending(x => x.SaleDate)
            .Take(10)
            .Select(x => new
            {
                x.SaleId,
                x.SaleDate,
                CustomerName = x.Customer != null ? x.Customer.FullName : string.Empty,
                x.FinalAmount,
                x.DiscountAmount,
                x.PaymentStatus
            })
            .ToListAsync();

        var recentPurchases = await purchasesQuery
            .OrderByDescending(x => x.PurchaseDate)
            .Take(10)
            .Select(x => new
            {
                x.PurchaseInvoiceId,
                x.PurchaseDate,
                VendorName = x.Vendor != null ? x.Vendor.VendorName : string.Empty,
                x.TotalAmount
            })
            .ToListAsync();

        return new SimpleFinancialReportDTO
        {
            Period = normalizedPeriod,
            PeriodLabel = periodLabel,
            TotalSalesRevenue = totalSalesRevenue,
            TotalPurchaseCost = totalPurchaseCost,
            EstimatedProfit = totalSalesRevenue - totalPurchaseCost,
            TotalDiscountGiven = totalDiscountGiven,
            SalesInvoiceCount = salesInvoiceCount,
            PurchaseInvoiceCount = purchaseInvoiceCount,
            RecentSalesInvoices = recentSales.Select(x => new FinancialSaleInvoiceDTO
            {
                SaleId = x.SaleId,
                InvoiceNumber = $"INV-{x.SaleId:D6}",
                SaleDate = x.SaleDate,
                CustomerName = string.IsNullOrWhiteSpace(x.CustomerName)
                    ? "Walk-in Customer"
                    : x.CustomerName,
                FinalAmount = x.FinalAmount,
                DiscountAmount = x.DiscountAmount,
                PaymentStatus = x.PaymentStatus.ToString()
            }).ToList(),
            RecentPurchaseInvoices = recentPurchases.Select(x => new FinancialPurchaseInvoiceDTO
            {
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                InvoiceNumber = $"PUR-{x.PurchaseInvoiceId:D6}",
                PurchaseDate = x.PurchaseDate,
                VendorName = string.IsNullOrWhiteSpace(x.VendorName)
                    ? "Unknown Vendor"
                    : x.VendorName,
                TotalAmount = x.TotalAmount
            }).ToList()
        };
    }

    public async Task<List<MonthlyRevenueDTO>> GetMonthlyRevenueAsync()
    {
        var year = DateTime.UtcNow.Year;

        var salesByMonth = await _context.Sales
            .Where(x => x.SaleDate.Year == year)
            .GroupBy(x => x.SaleDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                Sales = g.Sum(x => x.FinalAmount),
                Invoices = g.Count()
            })
            .ToListAsync();

        return Enumerable.Range(1, 12)
            .Select(month =>
            {
                var match = salesByMonth.FirstOrDefault(x => x.Month == month);

                return new MonthlyRevenueDTO
                {
                    Month = CultureInfo.InvariantCulture
                        .DateTimeFormat
                        .GetAbbreviatedMonthName(month)
                        .ToUpperInvariant(),
                    Sales = match?.Sales ?? 0,
                    Invoices = match?.Invoices ?? 0
                };
            })
            .ToList();
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

    private static (string Period, string PeriodLabel, DateTime StartDate, DateTime EndDate)
        GetReportPeriod(string period)
    {
        var now = DateTime.UtcNow;
        var normalizedPeriod = string.IsNullOrWhiteSpace(period)
            ? "monthly"
            : period.Trim().ToLowerInvariant();

        return normalizedPeriod switch
        {
            "daily" => (
                "daily",
                $"Daily Report - {now:dd MMM yyyy}",
                CreateUtcDate(now.Year, now.Month, now.Day),
                CreateUtcDate(now.Year, now.Month, now.Day).AddDays(1)),

            "yearly" => (
                "yearly",
                $"Yearly Report - {now.Year}",
                CreateUtcDate(now.Year, 1, 1),
                CreateUtcDate(now.Year + 1, 1, 1)),

            _ => (
                "monthly",
                $"Monthly Report - {now:MMMM yyyy}",
                CreateUtcDate(now.Year, now.Month, 1),
                CreateUtcDate(now.Year, now.Month, 1).AddMonths(1))
        };
    }

    private static DateTime CreateUtcDate(int year, int month, int day)
    {
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}
