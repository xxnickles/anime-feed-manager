using AnimeFeedManager.Features.ProcessState.Storage;

namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

public sealed record AppTableClient<T>(TableClient Client, ILogger Logger) where T : ITableEntity;

public interface ITableClientFactory
{
    Task<Result<AppTableClient<T>>> GetClient<T>(CancellationToken cancellationToken = default)  where T : ITableEntity;
}

public sealed class TableClientFactory : ITableClientFactory
{
    private readonly TableServiceClient _serviceClient;
    private readonly ILogger<TableClientFactory> _logger;

    public TableClientFactory(TableServiceClient serviceClient, ILogger<TableClientFactory> logger)
    {
        _serviceClient = serviceClient;
        _logger = logger;
    }
    
    public async Task<Result<AppTableClient<T>>> GetClient<T>(CancellationToken cancellationToken = default)  where T : ITableEntity
    {
        try
        {
            var client = _serviceClient.GetTableClient(TableNameFactory(typeof(T)));
            await client.CreateIfNotExistsAsync(cancellationToken);
            return Result<AppTableClient<T>>.Success(new AppTableClient<T>(client, _logger));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when creating a Table Client");;
            return HandledErrorResult<AppTableClient<T>>();
        }
      
    }
    
    private static string TableNameFactory(Type type)
    {
        return type.Name switch
        {
            // nameof(AnimeInfoStorage) => AzureTableMap.StoreTo.AnimeLibrary,
            // nameof(AnimeInfoWithImageStorage) => AzureTableMap.StoreTo.AnimeLibrary,
            // nameof(UpdateFeedAnimeInfoStorage) => AzureTableMap.StoreTo.AnimeLibrary,
            // nameof(AlternativeTitleStorage) =>  AzureTableMap.StoreTo.AlternativeTitles,
            // nameof(SubscriptionStorage) => AzureTableMap.StoreTo.Subscriptions,
            // nameof(SeasonStorage) => AzureTableMap.StoreTo.AvailableSeasons,
            // nameof(InterestedStorage) => AzureTableMap.StoreTo.InterestedSeries,
            // nameof(TitlesStorage) => AzureTableMap.StoreTo.FeedTitles,
            // nameof(ProcessedTitlesStorage) => AzureTableMap.StoreTo.ProcessedTitles,   
            // nameof(UserStorage) => AzureTableMap.StoreTo.Users,
            // nameof(OvaStorage) => AzureTableMap.StoreTo.OvaLibrary,
            // nameof(OvasSubscriptionStorage) => AzureTableMap.StoreTo.OvaSubscriptions,
            // nameof(MovieStorage) => AzureTableMap.StoreTo.MovieLibrary,
            // nameof(MoviesSubscriptionStorage) => AzureTableMap.StoreTo.MovieSubscriptions,
            // nameof(NotificationStorage) => AzureTableMap.StoreTo.Notifications,
            nameof(StateUpdateStorage) => AzureTableMap.StoreTo.StateUpdates,
            // nameof(LatestSeasonsStorage) => AzureTableMap.StoreTo.JsonStorage,
            _ => throw new ArgumentException($"There is not a defined table for the type {type.FullName}")
        };
    }
}