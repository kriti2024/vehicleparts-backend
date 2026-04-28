namespace VehicleParts.Application.DTOs.Part;

public class PartDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
}

public class CreatePartDto
{
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int VendorId { get; set; }
}

public class UpdatePartDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int VendorId { get; set; }
}
