using Microsoft.AspNetCore.Http;

namespace VehicleParts.Application.DTOs.Part;

public class PartDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? ImageUrl { get; set; }
    public string VehicleBrand { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
}

public class CreatePartDto
{
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int VendorId { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string VehicleBrand { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
}

public class UpdatePartDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int VendorId { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string VehicleBrand { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
}
