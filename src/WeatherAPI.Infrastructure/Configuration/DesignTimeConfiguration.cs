using Microsoft.Extensions.Configuration;

namespace WeatherAPI.Infrastructure.Configuration;

public static class DesignTimeConfiguration
{
    public static IConfigurationRoot Build()
    {
        EnvironmentLoader.LoadFromRoot();

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";
        var apiProjectPath = FindApiProjectPath();

        return new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string FindApiProjectPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            if (currentDirectory.Name == "WeatherAPI.Api" &&
                File.Exists(Path.Combine(currentDirectory.FullName, "appsettings.json")))
            {
                return currentDirectory.FullName;
            }

            var apiProjectPath = Path.Combine(
                currentDirectory.FullName,
                "src",
                "WeatherAPI.Api");

            if (Directory.Exists(apiProjectPath))
            {
                return apiProjectPath;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
