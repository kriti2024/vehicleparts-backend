using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Sale;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Services.Sales;

public class SalesService : ISalesService
{
    private readonly IApplicationDbContext _context;

    public SalesService(IApplicationDbContext context)
    {
        _context = context;
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
            // Get part from database (or use mock data for now)
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

            // Update stock
            part.StockQuantity -= item.Quantity;
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
            PaymentStatus = Domain.Enums.PaymentStatus.Paid,
            SaleItems = saleItems
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // Return SaleDTO
        var customer = await _context.Customers
            .FirstAsync(c => c.CustomerId == dto.CustomerId);

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

    //  MOCK DATA FOR PARTS (Replace later with Sujal's API)
    private async Task<Part?> GetPartAsync(int partId)
    {
        // Try to get from database first
        var part = await _context.Parts.FirstOrDefaultAsync(p => p.PartId == partId);

        if (part != null)
            return part;

        // If not in database, return mock data for testing
        var mockParts = new List<Part>
        {
            new Part { PartId = 1, PartName = "Brake Pad", Price = 1500, StockQuantity = 20 },
            new Part { PartId = 2, PartName = "Oil Filter", Price = 500, StockQuantity = 50 },
            new Part { PartId = 3, PartName = "Air Filter", Price = 800, StockQuantity = 30 },
            new Part { PartId = 4, PartName = "Spark Plug", Price = 300, StockQuantity = 100 },
            new Part { PartId = 5, PartName = "Engine Oil (1L)", Price = 1200, StockQuantity = 40 }
        };

        return mockParts.FirstOrDefault(p => p.PartId == partId);
    }
}