using System.Net;
using System.Text.Json;
using VehicleParts.Application.Common;
using VehicleParts.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VehicleParts.Application.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _logger.LogInformation(
                $"[REQUEST]: {context.Request.Method} {context.Request.Path}");

            await _next(context);

            _logger.LogInformation(
                $"[RESPONSE]: {context.Response.StatusCode}");
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, errors) = exception switch
        {
            NotFoundException ex =>
                (HttpStatusCode.NotFound,
                    new List<string> { ex.Message }),

            BadRequestException ex =>
                (HttpStatusCode.BadRequest,
                    new List<string> { ex.Message }),

            UnauthorizedException ex =>
                (HttpStatusCode.Unauthorized,
                    new List<string> { ex.Message }),

            ForbiddenException ex =>
                (HttpStatusCode.Forbidden,
                    new List<string> { ex.Message }),

            ValidationException ex =>
                (HttpStatusCode.BadRequest,
                    ex.Errors
                        .SelectMany(x =>
                            x.Value.Select(v =>
                                $"{x.Key}: {v}"))
                        .ToList()),

            _ =>
                (HttpStatusCode.InternalServerError,
                    new List<string>
                    {
                        "Internal server error."
                    })
        };

        _logger.LogError(
            exception,
            "Exception: {Message}",
            exception.Message);

        if (context.Response.HasStarted)
            return;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiResponse(
            data: null,
            errors: errors,
            statusCode: statusCode
        );

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase
        };

        var json =
            JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}