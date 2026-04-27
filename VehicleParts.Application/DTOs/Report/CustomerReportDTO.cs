using System;
using System.Collections.Generic;

namespace VehicleParts.Application.DTOs.Reports;

public class CustomerReportDTO
{
    public List<CustomerSummaryDTO> RegularCustomers { get; set; } = new();
    public List<CustomerSummaryDTO> HighSpenders { get; set; } = new();
    public List<CustomerSummaryDTO> PendingCreditCustomers { get; set; } = new();
}

public class CustomerSummaryDTO
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int TotalPurchases { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal PendingAmount { get; set; }
}
