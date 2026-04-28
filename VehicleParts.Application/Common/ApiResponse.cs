using System.Net;

namespace VehicleParts.Application.Common;

public class ApiResponse
{
    public object? Data { get; set; }

    public List<string> Errors { get; set; } = new();

    public int StatusCode { get; set; }

    public bool Success => StatusCode >= 200 && StatusCode < 300;

    public ApiResponse(
        object? data,
        List<string>? errors,
        HttpStatusCode statusCode)
    {
        Data = data;
        Errors = errors ?? new();
        StatusCode = (int)statusCode;
    }
}