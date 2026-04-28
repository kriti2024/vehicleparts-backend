using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt)
        GenerateToken(
            ApplicationUser user,
            IEnumerable<string> roles,
            string? customerId = null);
}