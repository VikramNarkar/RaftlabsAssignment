using ExternalProvider;
using ExternalProvider.Abstract;
using ExternalProvider.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

var services = new ServiceCollection();

// Configure logging
services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Register services
services.AddHttpClient("UserClient", client =>
{
    client.BaseAddress = new Uri("https://reqres.in/api/");
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
services.AddScoped<ExternalProviderConsoleApp.ExternalProviderConsoleAppRunner>();

var serviceProvider = services.BuildServiceProvider();

var runner = serviceProvider.GetRequiredService<ExternalProviderConsoleApp.ExternalProviderConsoleAppRunner>();
await runner.RunAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
