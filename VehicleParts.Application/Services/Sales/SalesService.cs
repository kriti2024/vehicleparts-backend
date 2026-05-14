using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Sale;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;
using VehicleParts.Domain.Enums;

namespace VehicleParts.Application.Services.Sales;

public class SalesService : ISalesService
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public SalesService(
        IApplicationDbContext context,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _context = context;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public async Task<SaleDTO> CreateSaleAsync(CreateSaleDTO dto)
    {
        // Verify customer exists
        var customerExists = await _context.Customers
            .AnyAsync(c => c.CustomerId == dto.CustomerId);

        if (!customerExists)
            throw new Exception($"Customer with ID {dto.CustomerId} not found");

        // Get parts and calculate prices
        var saleItems = new List<SaleItem>();
        decimal subTotal = 0;

        foreach (var item in dto.Items)
        {
            // Get part from database (also using mock data for testing)
            var part = await GetPartAsync(item.PartId);

            if (part == null)
                throw new Exception($"Part with ID {item.PartId} not found");

            if (part.StockQuantity < item.Quantity)
                throw new Exception($"Insufficient stock for {part.PartName}. Available: {part.StockQuantity}");

            var itemTotal = part.Price * item.Quantity;
            subTotal += itemTotal;

            saleItems.Add(new SaleItem
            {
                PartId = item.PartId,
                Quantity = item.Quantity,
                UnitPrice = part.Price
            });

            part.StockQuantity -= item.Quantity;

            // For Low Stock Notification
            if (part.StockQuantity < 10)
            {
                await _notificationService.NotifyLowStockAsync(part.PartId, part.PartName, part.StockQuantity);
            }
        }

        // FEATURE 16: LOYALTY DISCOUNT CALCULATION 
        decimal discountPercent = 0;
        decimal discountAmount = 0;
        decimal finalAmount = subTotal;

        if (subTotal > 5000)
        {
            discountPercent = 10;
            discountAmount = subTotal * 0.10m;
            finalAmount = subTotal - discountAmount;
        }

        // Create sale
        var sale = new Sale
        {
            CustomerId = dto.CustomerId,
            SaleDate = DateTime.UtcNow,
            SubTotal = subTotal,
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            PaymentStatus = ParsePaymentStatus(dto.PaymentStatus),
            SaleItems = saleItems
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // Return SaleDTO
        var customer = await _context.Customers
            .FirstAsync(c => c.CustomerId == dto.CustomerId);

        // FEATURE 11: Send invoice via email
        if (!string.IsNullOrEmpty(customer.Email))
        {
            await _emailService.SendEmailAsync(customer.Email, "Your Vehicle Parts Invoice", 
                $"Hello {customer.FullName}, thank you for your purchase of ${sale.FinalAmount}. Invoice ID: {sale.SaleId}");
        }

        return new SaleDTO
        {
            SaleId = sale.SaleId,
            CustomerId = sale.CustomerId,
            CustomerName = customer.FullName,
            SaleDate = sale.SaleDate,
            SubTotal = sale.SubTotal,
            DiscountPercent = sale.DiscountPercent,
            DiscountAmount = sale.DiscountAmount,
            FinalAmount = sale.FinalAmount,
            PaymentStatus = sale.PaymentStatus.ToString(),
            Items = saleItems.Select(si => new SaleItemDetailDTO
            {
                SaleItemId = si.SaleItemId,
                PartId = si.PartId,
                PartName = _context.Parts.First(p => p.PartId == si.PartId).PartName,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.Quantity * si.UnitPrice
            }).ToList()
        };
    }

    public async Task<SaleDTO?> GetSaleByIdAsync(int saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .FirstOrDefaultAsync(s => s.SaleId == saleId);

        if (sale == null) return null;

        return new SaleDTO
        {
            SaleId = sale.SaleId,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.FullName ?? "",
            SaleDate = sale.SaleDate,
            SubTotal = sale.SubTotal,
            DiscountPercent = sale.DiscountPercent,
            DiscountAmount = sale.DiscountAmount,
            FinalAmount = sale.FinalAmount,
            PaymentStatus = sale.PaymentStatus.ToString(),
            Items = sale.SaleItems.Select(si => new SaleItemDetailDTO
            {
                SaleItemId = si.SaleItemId,
                PartId = si.PartId,
                PartName = si.Part?.PartName ?? "",
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.Quantity * si.UnitPrice
            }).ToList()
        };
    }

    public async Task<InvoiceDTO?> GetInvoiceAsync(int saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .FirstOrDefaultAsync(s => s.SaleId == saleId);

        if (sale == null) return null;

        return new InvoiceDTO
        {
            SaleId = sale.SaleId,
            InvoiceNumber = $"INV-{sale.SaleId:D6}",
            InvoiceDate = sale.SaleDate,
            CustomerName = sale.Customer?.FullName ?? "",
            CustomerPhone = sale.Customer?.Phone ?? "",
            CustomerEmail = sale.Customer?.Email,
            Items = sale.SaleItems.Select(si => new InvoiceItemDTO
            {
                PartName = si.Part?.PartName ?? "",
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.Quantity * si.UnitPrice
            }).ToList(),
            SubTotal = sale.SubTotal,
            DiscountPercent = sale.DiscountPercent,
            DiscountAmount = sale.DiscountAmount,
            FinalAmount = sale.FinalAmount,
            PaymentStatus = sale.PaymentStatus.ToString()
        };
    }

    public async Task<List<SaleDTO>> GetCustomerSalesAsync(int customerId)
    {
        return await _context.Sales
            .Where(s => s.CustomerId == customerId)
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .Select(s => new SaleDTO
            {
                SaleId = s.SaleId,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.FullName : "",
                SaleDate = s.SaleDate,
                SubTotal = s.SubTotal,
                DiscountPercent = s.DiscountPercent,
                DiscountAmount = s.DiscountAmount,
                FinalAmount = s.FinalAmount,
                PaymentStatus = s.PaymentStatus.ToString(),
                Items = s.SaleItems.Select(si => new SaleItemDetailDTO
                {
                    SaleItemId = si.SaleItemId,
                    PartId = si.PartId,
                    PartName = si.Part != null ? si.Part.PartName : "",
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice,
                    TotalPrice = si.Quantity * si.UnitPrice
                }).ToList()
            })
            .ToListAsync();
    }

    //  Get part details from database with Sujal's API
    private async Task<Part?> GetPartAsync(int partId)
    {
        return await _context.Parts
            .FirstOrDefaultAsync(p => p.PartId == partId);
    }

    private static PaymentStatus ParsePaymentStatus(string? paymentStatus)
    {
        return Enum.TryParse<PaymentStatus>(paymentStatus, true, out var parsed)
            ? parsed
            : PaymentStatus.Paid;
    }

    public async Task SendInvoiceEmailAsync(int saleId)
    {
        // Get invoice data
        var invoice = await GetInvoiceAsync(saleId);

        if (invoice == null)
            throw new Exception($"Invoice for Sale ID {saleId} not found");

        if (string.IsNullOrEmpty(invoice.CustomerEmail))
            throw new Exception("Customer email not available");

        // Build email content
        var subject = $"Invoice #{invoice.InvoiceNumber}";

        var body = $@"
Vehicle Parts Invoice

Hello {invoice.CustomerName},

Thank you for your purchase.

Invoice Details
----------------------------------
Invoice Number : {invoice.InvoiceNumber}
Date           : {invoice.InvoiceDate:dd-MM-yyyy}
Payment Status : {invoice.PaymentStatus}

Purchased Items
----------------------------------
{string.Join("\n", invoice.Items.Select(i =>
        $"{i.PartName} | Qty: {i.Quantity} | Rs. {i.TotalPrice}"))}

----------------------------------
Subtotal        : Rs. {invoice.SubTotal}
Discount ({invoice.DiscountPercent}%): Rs. {invoice.DiscountAmount}
Final Amount    : Rs. {invoice.FinalAmount}
----------------------------------

Thank you for choosing Vehicle Parts System.

Regards,
Vehicle Parts Team
";

        // Send email
        await _emailService.SendEmailAsync(
            invoice.CustomerEmail,
            subject,
            body
        );
    }
}
