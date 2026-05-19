using VehicleParts.Application.DTOs.Sale;

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

public class DetailedFinancialReportDTO
{
    public decimal SalesAmount { get; set; }
    public int InvoicesCount { get; set; }
    public List<SaleDTO> Data { get; set; } = new();
}

public class SimpleFinancialReportDTO
{
    public string Period { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
    public decimal TotalSalesRevenue { get; set; }
    public decimal TotalPurchaseCost { get; set; }
    public decimal EstimatedProfit { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public int SalesInvoiceCount { get; set; }
    public int PurchaseInvoiceCount { get; set; }
    public List<FinancialSaleInvoiceDTO> RecentSalesInvoices { get; set; } = new();
    public List<FinancialPurchaseInvoiceDTO> RecentPurchaseInvoices { get; set; } = new();
}

public class FinancialSaleInvoiceDTO
{
    public int SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class FinancialPurchaseInvoiceDTO
{
    public int PurchaseInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
