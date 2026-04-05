using DotNetEnv;
using System.IO;

namespace WeatherAPI.Infrastructure.Configuration;

public static class EnvironmentLoader
{
    public static void LoadFromRoot()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var envFilePath = Path.Combine(currentDirectory.FullName, ".env");

            if (File.Exists(envFilePath))
            {
                Env.Load(envFilePath);
                return;
            }

            currentDirectory = currentDirectory.Parent;
        }
    }
}
