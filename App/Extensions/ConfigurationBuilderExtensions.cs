using Microsoft.Extensions.Configuration;

namespace App.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static void AddJsonFile(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.SetBasePath(PathExtensions.GetDirectoryPath());
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "DEV";
        configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configurationBuilder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
    }
}