using System.Security.Claims;
using VehicleParts.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace VehicleParts.Infrastructure.Auth;

public class CurrentUserService(
    IHttpContextAccessor accessor)
    : ICurrentUserService
{
    public string? UserId =>
        accessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email =>
        accessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated =>
        accessor.HttpContext?.User?
        .Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        accessor.HttpContext?.User?
        .IsInRole(role) ?? false;
}