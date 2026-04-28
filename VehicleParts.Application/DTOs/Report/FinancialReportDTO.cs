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