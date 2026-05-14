using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public decimal PendingCredit { get; set; } = 0;
    public DateTime? LastPaymentDate { get; set; }

    // Navigation properties
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<ServiceBooking> ServiceBookings { get; set; } = new List<ServiceBooking>();
    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();
    public ICollection<ServiceReview> ServiceReviews { get; set; } = new List<ServiceReview>();
}