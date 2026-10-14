using System.Net;
using System.Text.Json;
using QRHub.Application.Common.Exceptions;
using QRHub.Application.Common.Models;

namespace QRHub.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException apiEx)
        {
            _logger.LogWarning("API Exception occurred: {Message}", apiEx.Message);
            await HandleApiExceptionAsync(context, apiEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleUnhandledExceptionAsync(context, ex);
        }
    }

    private static async Task HandleApiExceptionAsync(HttpContext context, ApiException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)exception.StatusCode;

        var response = ApiResponse.Fail(exception.Message, exception.Errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static async Task HandleUnhandledExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = ApiResponse.Fail("An unexpected internal error occurred. Please try again later.");
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
