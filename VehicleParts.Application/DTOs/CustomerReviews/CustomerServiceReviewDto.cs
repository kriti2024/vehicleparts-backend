using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.CustomerReviews;

public class CreateServiceReviewDto
{
    [Required]
    public int CustomerId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}
