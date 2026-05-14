using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.DTOs.Reports;
using VehicleParts.Application.DTOs.Sale;
using VehicleParts.Application.DTOs.Vehicle;

namespace VehicleParts.Controllers;

// Keeps staff workflows usable when PostgreSQL is unavailable or seeded tables are empty.
public static class StaffFallbackStore
{
    private static readonly object Sync = new();
    private static int _nextCustomerId = 3;
    private static int _nextVehicleId = 3;
    private static int _nextSaleId = 1003;

    private static readonly List<CustomerDTO> Customers =
    [
        new() { CustomerId = 1, FullName = "Regular Customer", Phone = "9800000000", Email = "customer@email.com" },
        new() { CustomerId = 2, FullName = "Fleet Buyer", Phone = "9811111111", Email = "fleet@email.com" }
    ];

    private static readonly List<VehicleDTO> Vehicles =
    [
        new() { VehicleId = 1, CustomerId = 1, CustomerName = "Regular Customer", VehicleNumber = "BA 12 PA 1234", Model = "Toyota Corolla" },
        new() { VehicleId = 2, CustomerId = 2, CustomerName = "Fleet Buyer", VehicleNumber = "BAG 4412", Model = "Hyundai Creta" }
    ];

    private static readonly Dictionary<int, (string Name, decimal Price)> Parts = new()
    {
        [101] = ("Brake Pad Set", 3200),
        [102] = ("Engine Oil 5W-30", 1850),
        [103] = ("Spark Plug", 950),
        [104] = ("Air Filter", 780),
        [107] = ("Car Battery", 7600)
    };

    private static readonly List<SaleDTO> Sales =
    [
        new()
        {
            SaleId = 1001,
            CustomerId = 1,
            CustomerName = "Regular Customer",
            SaleDate = DateTime.UtcNow.Date.AddDays(-5),
            SubTotal = 5050,
            DiscountPercent = 5,
            DiscountAmount = 252.5m,
            FinalAmount = 4797.5m,
            PaymentStatus = "Paid",
            Items =
            [
                new() { SaleItemId = 1, PartId = 101, PartName = "Brake Pad Set", Quantity = 1, UnitPrice = 3200, TotalPrice = 3200 },
                new() { SaleItemId = 2, PartId = 102, PartName = "Engine Oil 5W-30", Quantity = 1, UnitPrice = 1850, TotalPrice = 1850 }
            ]
        },
        new()
        {
            SaleId = 1002,
            CustomerId = 2,
            CustomerName = "Fleet Buyer",
            SaleDate = DateTime.UtcNow.Date.AddDays(-2),
            SubTotal = 15200,
            DiscountPercent = 10,
            DiscountAmount = 1520,
            FinalAmount = 13680,
            PaymentStatus = "Credit due",
            Items =
            [
                new() { SaleItemId = 3, PartId = 107, PartName = "Car Battery", Quantity = 2, UnitPrice = 7600, TotalPrice = 15200 }
            ]
        }
    ];

    public static List<CustomerDTO> GetCustomers()
    {
        lock (Sync)
        {
            return Customers.Select(CloneCustomer).ToList();
        }
    }

    public static CustomerDTO? GetCustomer(int id)
    {
        lock (Sync)
        {
            return Customers.Where(c => c.CustomerId == id).Select(CloneCustomer).FirstOrDefault();
        }
    }

    public static CustomerDTO CreateCustomer(CreateCustomerDTO dto)
    {
        lock (Sync)
        {
            var customer = new CustomerDTO
            {
                CustomerId = _nextCustomerId++,
                FullName = dto.FullName.Trim(),
                Phone = dto.Phone.Trim(),
                Email = dto.Email
            };

            Customers.Add(customer);
            return CloneCustomer(customer);
        }
    }

    public static VehicleDTO AddVehicle(CreateVehicleDTO dto)
    {
        lock (Sync)
        {
            var customer = Customers.FirstOrDefault(c => c.CustomerId == dto.CustomerId)
                ?? throw new InvalidOperationException("Customer not found.");

            var vehicle = new VehicleDTO
            {
                VehicleId = _nextVehicleId++,
                CustomerId = customer.CustomerId,
                CustomerName = customer.FullName,
                VehicleNumber = dto.VehicleNumber.Trim(),
                Model = dto.Model.Trim()
            };

            Vehicles.Add(vehicle);
            return CloneVehicle(vehicle);
        }
    }

    public static CustomerWithVehiclesDTO? GetCustomerWithVehicles(int id)
    {
        lock (Sync)
        {
            var customer = Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null) return null;

            return new CustomerWithVehiclesDTO
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Phone = customer.Phone,
                Email = customer.Email,
                Vehicles = Vehicles
                    .Where(v => v.CustomerId == id)
                    .Select(CloneVehicle)
                    .ToList()
            };
        }
    }

    public static List<VehicleDTO> GetCustomerVehicles(int id)
    {
        lock (Sync)
        {
            return Vehicles.Where(v => v.CustomerId == id).Select(CloneVehicle).ToList();
        }
    }

    public static List<CustomerSearchDTO> SearchCustomers(string keyword)
    {
        lock (Sync)
        {
            var term = keyword.Trim().ToLowerInvariant();

            return Customers
                .SelectMany(customer =>
                {
                    var customerVehicles = Vehicles.Where(v => v.CustomerId == customer.CustomerId).DefaultIfEmpty();
                    return customerVehicles.Select(vehicle => new { customer, vehicle });
                })
                .Where(row =>
                    row.customer.CustomerId.ToString().Contains(term) ||
                    row.customer.FullName.ToLowerInvariant().Contains(term) ||
                    row.customer.Phone.ToLowerInvariant().Contains(term) ||
                    (row.customer.Email ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (row.vehicle?.VehicleNumber ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (row.vehicle?.Model ?? string.Empty).ToLowerInvariant().Contains(term))
                .Select(row => new CustomerSearchDTO
                {
                    CustomerId = row.customer.CustomerId,
                    FullName = row.customer.FullName,
                    Phone = row.customer.Phone,
                    Email = row.customer.Email,
                    VehicleNumber = row.vehicle?.VehicleNumber ?? string.Empty,
                    Model = row.vehicle?.Model ?? string.Empty
                })
                .GroupBy(c => c.CustomerId)
                .Select(group => group.First())
                .ToList();
        }
    }

    public static SaleDTO CreateSale(CreateSaleDTO dto)
    {
        lock (Sync)
        {
            var customer = Customers.FirstOrDefault(c => c.CustomerId == dto.CustomerId)
                ?? throw new InvalidOperationException("Customer not found.");

            var items = dto.Items.Select((item, index) =>
            {
                var part = Parts.TryGetValue(item.PartId, out var found)
                    ? found
                    : ($"Part #{item.PartId}", 1000m);
                return new SaleItemDetailDTO
                {
                    SaleItemId = index + 1,
                    PartId = item.PartId,
                    PartName = part.Item1,
                    Quantity = item.Quantity,
                    UnitPrice = part.Item2,
                    TotalPrice = part.Item2 * item.Quantity
                };
            }).ToList();

            var subtotal = items.Sum(item => item.TotalPrice);
            var discountPercent = subtotal >= 10000 ? 10 : subtotal >= 5000 ? 5 : 0;
            var discountAmount = subtotal * discountPercent / 100;
            var sale = new SaleDTO
            {
                SaleId = _nextSaleId++,
                CustomerId = customer.CustomerId,
                CustomerName = customer.FullName,
                SaleDate = DateTime.UtcNow,
                SubTotal = subtotal,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                FinalAmount = subtotal - discountAmount,
                PaymentStatus = "Paid",
                Items = items
            };

            Sales.Insert(0, sale);
            return CloneSale(sale);
        }
    }

    public static SaleDTO? GetSale(int id)
    {
        lock (Sync)
        {
            return Sales.Where(s => s.SaleId == id).Select(CloneSale).FirstOrDefault();
        }
    }

    public static List<SaleDTO> GetCustomerSales(int customerId)
    {
        lock (Sync)
        {
            return Sales.Where(s => s.CustomerId == customerId).Select(CloneSale).ToList();
        }
    }

    public static InvoiceDTO? GetInvoice(int saleId)
    {
        lock (Sync)
        {
            var sale = Sales.FirstOrDefault(s => s.SaleId == saleId);
            if (sale == null) return null;

            var customer = Customers.FirstOrDefault(c => c.CustomerId == sale.CustomerId);
            return new InvoiceDTO
            {
                SaleId = sale.SaleId,
                InvoiceNumber = $"INV-{sale.SaleId}",
                InvoiceDate = sale.SaleDate,
                CustomerName = sale.CustomerName,
                CustomerPhone = customer?.Phone ?? string.Empty,
                CustomerEmail = customer?.Email,
                Items = sale.Items.Select(item => new InvoiceItemDTO
                {
                    PartName = item.PartName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList(),
                SubTotal = sale.SubTotal,
                DiscountPercent = sale.DiscountPercent,
                DiscountAmount = sale.DiscountAmount,
                FinalAmount = sale.FinalAmount,
                PaymentStatus = sale.PaymentStatus
            };
        }
    }

    public static List<VehicleParts.Application.DTOs.Part.PartDto> GetParts()
    {
        lock (Sync)
        {
            return Parts.Select(part => new VehicleParts.Application.DTOs.Part.PartDto
            {
                PartId = part.Key,
                PartName = part.Value.Item1,
                Price = part.Value.Item2,
                StockQuantity = part.Key switch
                {
                    101 => 14,
                    102 => 18,
                    103 => 24,
                    104 => 8,
                    107 => 6,
                    _ => 10
                },
                VendorId = 1,
                VendorName = "Axleworks Stock"
            }).ToList();
        }
    }

    public static CustomerReportDTO GetCustomerReport()
    {
        lock (Sync)
        {
            var summaries = Customers.Select(customer =>
            {
                var sales = Sales.Where(s => s.CustomerId == customer.CustomerId).ToList();
                return new CustomerSummaryDTO
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    Email = customer.Email ?? string.Empty,
                    Phone = customer.Phone ?? string.Empty,
                    TotalPurchases = sales.Count,
                    TotalSpent = sales.Sum(s => s.FinalAmount),
                    PendingAmount = sales.Where(s => s.PaymentStatus.Contains("Credit", StringComparison.OrdinalIgnoreCase)).Sum(s => s.FinalAmount)
                };
            }).ToList();

            return new CustomerReportDTO
            {
                RegularCustomers = summaries.Where(s => s.TotalPurchases >= 1).OrderByDescending(s => s.TotalPurchases).ToList(),
                HighSpenders = summaries.OrderByDescending(s => s.TotalSpent).Take(10).ToList(),
                PendingCreditCustomers = summaries.Where(s => s.PendingAmount > 0).OrderByDescending(s => s.PendingAmount).ToList()
            };
        }
    }

    private static CustomerDTO CloneCustomer(CustomerDTO customer) => new()
    {
        CustomerId = customer.CustomerId,
        FullName = customer.FullName,
        Phone = customer.Phone,
        Email = customer.Email
    };

    private static VehicleDTO CloneVehicle(VehicleDTO vehicle) => new()
    {
        VehicleId = vehicle.VehicleId,
        CustomerId = vehicle.CustomerId,
        CustomerName = vehicle.CustomerName,
        VehicleNumber = vehicle.VehicleNumber,
        Model = vehicle.Model
    };

    private static SaleDTO CloneSale(SaleDTO sale) => new()
    {
        SaleId = sale.SaleId,
        CustomerId = sale.CustomerId,
        CustomerName = sale.CustomerName,
        SaleDate = sale.SaleDate,
        SubTotal = sale.SubTotal,
        DiscountPercent = sale.DiscountPercent,
        DiscountAmount = sale.DiscountAmount,
        FinalAmount = sale.FinalAmount,
        PaymentStatus = sale.PaymentStatus,
        Items = sale.Items.Select(item => new SaleItemDetailDTO
        {
            SaleItemId = item.SaleItemId,
            PartId = item.PartId,
            PartName = item.PartName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice
        }).ToList()
    };
}
