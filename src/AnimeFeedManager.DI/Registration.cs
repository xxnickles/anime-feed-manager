using AnimeFeedManager.Application.TvAnimeLibrary.Queries;
using AnimeFeedManager.Services.Collectors.AniDb;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Services.Collectors.SubsPlease;
using AnimeFeedManager.Storage.Infrastructure;
using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimeFeedManager.DI;

public static class Registration
{
    private static Func<IServiceProvider, Func<Type, string>> TableNameFactory => _ => type =>
    {
        return type.Name switch
        {
            nameof(AnimeInfoStorage) => AzureTable.TableMap.AnimeLibrary,
            nameof(SubscriptionStorage) => AzureTable.TableMap.Subscriptions,
            nameof(SeasonStorage) => AzureTable.TableMap.AvailableSeasons,
            nameof(InterestedStorage) => AzureTable.TableMap.InterestedSeries,
            nameof(TitlesStorage) => AzureTable.TableMap.FeedTitles,
            nameof(ProcessedTitlesStorage) => AzureTable.TableMap.ProcessedTitles,   
            nameof(UserStorage) => AzureTable.TableMap.Users,
            nameof(OvaStorage) => AzureTable.TableMap.OvaLibrary,
            nameof(OvasSubscriptionStorage) => AzureTable.TableMap.OvaSubscriptions,
            nameof(MovieStorage) => AzureTable.TableMap.MovieLibrary,
            nameof(MoviesSubscriptionStorage) => AzureTable.TableMap.MovieSubscriptions,
            nameof(NotificationStorage) => AzureTable.TableMap.Notifications,
            nameof(UpdateStateStorage) => AzureTable.TableMap.UpdateState,
            _ => throw new ArgumentException($"There is not a defined table for the type {type.FullName}")
        };
    };

    public static IServiceCollection RegisterStorage(this IServiceCollection services, string connectionString)
    {
        
        services.Configure<AzureBlobStorageOptions>(options =>
        {
            options.StorageConnectionString = connectionString;
        });

        services.TryAddSingleton<IImagesStore, AzureStorageBlobStore>();
        
        var tableClient = new TableServiceClient(connectionString);
        services.AddSingleton<IQueueResolver, QueueResolver>();
        services.AddSingleton<IDomainPostman, AzureQueueMessages>();
        services.AddSingleton(TableNameFactory);
        services.AddSingleton(tableClient);
        services.RegisterRepositories();

        return services;
    }

    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(conf => conf.RegisterServicesFromAssembly(typeof(GetSeasonCollectionQry).Assembly));
        return services;
    }

    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        services.AddScoped<ITvSeriesProvider, TvSeriesProvider>();
        services.AddScoped<IOvasProvider, OvasProvider>();
        services.AddScoped<IMoviesProvider, MoviesProvider>();
        services.AddScoped<IFeedProvider, FeedProvider>();
        return services;
    }
}