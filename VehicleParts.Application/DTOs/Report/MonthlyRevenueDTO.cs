namespace VehicleParts.Application.DTOs.Reports;

public class MonthlyRevenueDTO
{
    public string Month { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public int Invoices { get; set; }
}
