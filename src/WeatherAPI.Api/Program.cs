using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Api.Common;
using WeatherAPI.Api.Middleware;
using WeatherAPI.Infrastructure.Configuration;
using WeatherAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

EnvironmentLoader.LoadFromRoot();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(modelStateEntry => modelStateEntry.Value?.Errors.Count > 0)
            .ToDictionary(
                modelStateEntry => modelStateEntry.Key,
                modelStateEntry => modelStateEntry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The input was invalid."
                        : error.ErrorMessage)
                    .ToArray());

        var errorResponse = new ErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Validation failed.",
            Errors = errors
        };

        return new BadRequestObjectResult(errorResponse);
    };
});

var app = builder.Build();

app.UseGlobalExceptionMiddleware();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
