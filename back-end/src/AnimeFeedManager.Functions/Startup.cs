using System;
using AnimeFeedManager.DI;
using AnimeFeedManager.Functions;
using AnimeFeedManager.Functions.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AnimeFeedManager.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");


        builder.Services
            .RegisterStorage(storageConnection)
            .RegisterAppServices()
            .RegisterApplicationServices()
            .AddSingleton<ISendGridConfiguration>(GetSendGridConfiguration());
    }

    private SendGridConfiguration GetSendGridConfiguration()
    {
        var defaultFromEmail = Environment.GetEnvironmentVariable("FromEmail") ?? "test@test.com";
        var defaultFromName = Environment.GetEnvironmentVariable("FromName") ?? "Test";
        bool.TryParse(Environment.GetEnvironmentVariable("Sandbox"), out var sandbox);
        return new SendGridConfiguration(defaultFromEmail, defaultFromName, sandbox);
    }
}