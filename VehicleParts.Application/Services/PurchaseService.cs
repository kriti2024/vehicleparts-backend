using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Purchase;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Services;

public class PurchaseService(IApplicationDbContext context) : IPurchaseService
{
    public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllPurchaseInvoicesAsync()
    {
        return await context.PurchaseInvoices
            .Include(pi => pi.Vendor)
            .Include(pi => pi.PurchaseInvoiceItems)
                .ThenInclude(pii => pii.Part)
            .Select(pi => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = pi.PurchaseInvoiceId,
                VendorId = pi.VendorId,
                VendorName = pi.Vendor != null ? pi.Vendor.VendorName : null,
                PurchaseDate = pi.PurchaseDate,
                TotalAmount = pi.TotalAmount,
                Items = pi.PurchaseInvoiceItems.Select(pii => new PurchaseInvoiceItemDto
                {
                    PurchaseInvoiceItemId = pii.PurchaseInvoiceItemId,
                    PartId = pii.PartId,
                    PartName = pii.Part != null ? pii.Part.PartName : null,
                    Quantity = pii.Quantity,
                    UnitPrice = pii.UnitPrice,
                    TotalPrice = pii.TotalPrice
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<PurchaseInvoiceDto?> GetPurchaseInvoiceByIdAsync(int id)
    {
        var pi = await context.PurchaseInvoices
            .Include(pi => pi.Vendor)
            .Include(pi => pi.PurchaseInvoiceItems)
                .ThenInclude(pii => pii.Part)
            .FirstOrDefaultAsync(x => x.PurchaseInvoiceId == id);

        if (pi == null) return null;

        return new PurchaseInvoiceDto
        {
            PurchaseInvoiceId = pi.PurchaseInvoiceId,
            VendorId = pi.VendorId,
            VendorName = pi.Vendor != null ? pi.Vendor.VendorName : null,
            PurchaseDate = pi.PurchaseDate,
            TotalAmount = pi.TotalAmount,
            Items = pi.PurchaseInvoiceItems.Select(pii => new PurchaseInvoiceItemDto
            {
                PurchaseInvoiceItemId = pii.PurchaseInvoiceItemId,
                PartId = pii.PartId,
                PartName = pii.Part != null ? pii.Part.PartName : null,
                Quantity = pii.Quantity,
                UnitPrice = pii.UnitPrice,
                TotalPrice = pii.TotalPrice
            }).ToList()
        };
    }

    public async Task<PurchaseInvoiceDto> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceDto dto)
    {
        var invoice = new PurchaseInvoice
        {
            VendorId = dto.VendorId,
            PurchaseDate = dto.PurchaseDate,
            TotalAmount = dto.Items.Sum(x => x.Quantity * x.UnitPrice)
        };

        foreach (var itemDto in dto.Items)
        {
            var item = new PurchaseInvoiceItem
            {
                PartId = itemDto.PartId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice
            };
            invoice.PurchaseInvoiceItems.Add(item);

            // FEATURE 4: Update Stock Quantity
            var part = await context.Parts.FindAsync(itemDto.PartId);
            if (part != null)
            {
                part.StockQuantity += itemDto.Quantity;
            }
        }

        context.PurchaseInvoices.Add(invoice);
        await context.SaveChangesAsync();

        return await GetPurchaseInvoiceByIdAsync(invoice.PurchaseInvoiceId) ?? new PurchaseInvoiceDto();
    }
}
