namespace VehicleParts.Application.DTOs.Reports;

public class FinancialReportDTO
{
    public decimal DailySales { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal YearlySales { get; set; }

    public int DailyInvoices { get; set; }
    public int MonthlyInvoices { get; set; }
    public int YearlyInvoices { get; set; }
}

public class MonthlyRevenueDTO
{
    public string Month { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public int Invoices { get; set; }
}
