using ExternalProvider;
using ExternalProvider.Abstract;
using ExternalProvider.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using ExternalProvider.Models.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ExternalProviderConsoleApp;

var services = new ServiceCollection();

// Configuring configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory()) // To find appsettings.json
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Registering IOptions<ExternalApiSettings>
services.Configure<ExternalApiSettings>(configuration.GetSection("ExternalApiSettings"));

// Configuring logging
services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Configuring HttpClient with Polly Retry Policy
services.AddHttpClient("UserClient", (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ExternalApiSettings>>();
    var settings = options.Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Add("x-api-key", settings.ApiKey);
})
.AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message}");
        }));

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddScoped<IExternalUserService, ExternalUserService>();
services.AddScoped<ExternalProviderConsoleAppRunner>();

var serviceProvider = services.BuildServiceProvider();

var runner = serviceProvider.GetRequiredService<ExternalProviderConsoleAppRunner>();
await runner.RunAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
