namespace VehicleParts.Application.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(string email, string password);
    Task RegisterAsync(string email, string password, string fullName);
}