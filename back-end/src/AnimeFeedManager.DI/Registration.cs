using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Services.Collectors.SubsPlease;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using AnimeFeedManager.Services.Collectors.AniDb;
using Azure.Data.Tables;

namespace AnimeFeedManager.DI;

public static class Registration
{
    public static Func<IServiceProvider, Func<Type, string>> TableNameFactory => _ => type =>
    {
        return type.Name switch
        {
            nameof(AnimeInfoStorage) => AzureTable.TableMap.AnimeLibrary,
            nameof(SubscriptionStorage) => AzureTable.TableMap.Subscriptions,
            nameof(SeasonStorage) => AzureTable.TableMap.AvailableSeasons,
            nameof(InterestedStorage) => AzureTable.TableMap.InterestedSeries,
            nameof(TitlesStorage) => AzureTable.TableMap.FeedTitles,
            nameof(ProcessedTitlesStorage) => AzureTable.TableMap.ProcessedTitles,        
            _ => throw new ArgumentException($"There is not a defined table for the type {type.FullName}"),
        };
    };

    public static IServiceCollection RegisterStorage(this IServiceCollection services, string connectionString)
    {
        var tableClient = new TableServiceClient(connectionString);
        services.AddSingleton(TableNameFactory);
        services.AddSingleton(tableClient);
        services.RegisterRepositories();

        return services;
    }

    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(typeof(GetSeasonCollection).Assembly);
        return services;
    }

    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        services.AddScoped<IExternalLibraryProvider, LibraryProvider>();
        services.AddScoped<IFeedProvider, FeedProvider>();
        return services;
    }
}