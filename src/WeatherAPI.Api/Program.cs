using DotNetEnv;
using WeatherAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
