using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherAPI.Api.Common;

namespace WeatherAPI.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException exception)
        {
            logger.LogWarning(
                exception,
                "A bad request error occurred for {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status400BadRequest,
                exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "An unhandled exception occurred for {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
