using App.Extensions;
using Bullseye;
using Lib;
using Lib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using AppHost = Microsoft.Extensions.Hosting.Host;
using static System.ConsoleColor;

namespace App;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using (var host = CreateHostBuilder(args).Build())
        {
            var targets = new Targets();
            var client = host.Services.GetRequiredService<IAzureDevopsClient>();

            targets.Add(TargetTypes.Default, dependsOn: new List<string>
            {
                TargetTypes.GetPullRequestsBySdk,
                TargetTypes.GetPullRequestsByRest,
            });

            targets.Add(TargetTypes.GetPullRequestsBySdk, async () =>
            {
                var results = await client.GetAzurePullRequestsAsync(AzureDevopsChoice.Sdk);
                var resultsDump = ObjectDumper.Dump(results);
                Yellow.WriteLine($"Found {results.Count} pull-requests");
                Console.WriteLine(resultsDump);
            });

            targets.Add(TargetTypes.GetPullRequestsByRest, async () =>
            {
                var results = await client.GetAzurePullRequestsAsync(AzureDevopsChoice.Rest);
                var resultsDump = ObjectDumper.Dump(results);
                Yellow.WriteLine($"Found {results.Count} pull-requests");
                Console.WriteLine(resultsDump);
            });

            await targets.RunAndExitAsync(args);
        }

        Console.WriteLine("Press any key to exit !");
        Console.ReadKey();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        AppHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile();
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureLogging((hostingContext, loggingBuilder) =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsoleLogger();
                loggingBuilder.AddNonGenericLogger();
                loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            })
            .ConfigureServices((hostingContext, services) =>
            {
                services.AddHttpClient<IAzureDevopsClient, AzureDevopsClient>();
                services.AddTransient<GitHttpClient>(serviceProvider =>
                {
                    var settings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;
                    ObjectDumper.Dump(settings);
                    var credentials = new VssBasicCredential(string.Empty, settings.PersonalAccessToken);
                    var connection = new VssConnection(new Uri($"{settings.AzureDevopsUrl}/{settings.OrganizationName}"), credentials);
                    return connection.GetClient<GitHttpClient>();
                });
                services.Configure<Settings>(hostingContext.Configuration.GetSection(nameof(Settings)));
            })
            .UseConsoleLifetime();

    private static class TargetTypes
    {
        public const string Default = "Default";
        public const string GetPullRequestsBySdk = "GetPullRequestsBySdk";
        public const string GetPullRequestsByRest = "GetPullRequestsByRest";
    }
}