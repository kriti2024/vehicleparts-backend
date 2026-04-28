using Microsoft.EntityFrameworkCore;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<Part> Parts { get; }
    DbSet<Vendor> Vendors { get; }
    DbSet<PurchaseInvoice> PurchaseInvoices { get; }
    DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ServiceBooking> ServiceBookings { get; }
    DbSet<PartRequest> PartRequests { get; }
    DbSet<ServiceReview> ServiceReviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}