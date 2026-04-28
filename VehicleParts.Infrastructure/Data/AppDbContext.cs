using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Part -> Vendor
        modelBuilder.Entity<Part>()
            .HasOne(p => p.Vendor)
            .WithMany()
            .HasForeignKey(p => p.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        // PurchaseInvoice -> Vendor
        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Vendor)
            .WithMany()
            .HasForeignKey(pi => pi.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        // PurchaseInvoice -> PurchaseInvoiceItems
        modelBuilder.Entity<PurchaseInvoice>()
            .HasMany(pi => pi.PurchaseInvoiceItems)
            .WithOne(pii => pii.PurchaseInvoice)
            .HasForeignKey(pii => pii.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // PurchaseInvoiceItem -> Part
        modelBuilder.Entity<PurchaseInvoiceItem>()
            .HasOne(pii => pii.Part)
            .WithMany()
            .HasForeignKey(pii => pii.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        // Vehicle -> Customer
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sale -> Customer
        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
        // Vehicle -> Customer
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sale -> Customer
        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sale -> SaleItems
        modelBuilder.Entity<Sale>()
            .HasMany(s => s.SaleItems)
            .WithOne(si => si.Sale)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // SaleItem -> Part
        modelBuilder.Entity<SaleItem>()
            .HasOne(si => si.Part)
            .WithMany()
            .HasForeignKey(si => si.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}