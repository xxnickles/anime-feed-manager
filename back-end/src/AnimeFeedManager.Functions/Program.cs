﻿using AnimeFeedManager.DI;
using AnimeFeedManager.Functions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AnimeFeedManager.Functions;

public static class Program
{
    public static void Main()
    {
        var storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(s =>
            {
                s.RegisterStorage(storageConnection);
                s.RegisterAppServices();
                s.RegisterApplicationServices();
                s.AddSingleton<ISendGridConfiguration>(GetSendGridConfiguration());
            })
            .Build();

        host.Run();
    }


    

    private static SendGridConfiguration GetSendGridConfiguration()
    {
        var defaultFromEmail = Environment.GetEnvironmentVariable("FromEmail") ?? "test@test.com";
        var defaultFromName = Environment.GetEnvironmentVariable("FromName") ?? "Test";
        bool.TryParse(Environment.GetEnvironmentVariable("Sandbox"), out var sandbox);
        return new SendGridConfiguration(defaultFromEmail, defaultFromName, sandbox);
    }
}