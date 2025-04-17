using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Old.Features.Movies.Subscriptions.Types;
using AnimeFeedManager.Old.Features.Notifications.Types;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.Types;
using AnimeFeedManager.Old.Features.State.Types;
using AnimeFeedManager.Old.Features.Tv.Scrapping.Titles.Types;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Old.Features.Tv.Types;
using AnimeFeedManager.Old.Features.Users.Types;

namespace AnimeFeedManager.Old.Features.Infrastructure.TableStorage;

public sealed class TableClientFactory<T>(TableServiceClient serviceClient) : ITableClientFactory<T>
    where T : ITableEntity
{
    public async Task<Either<DomainError,TableClient>> GetClient()
    {
        try
        {
            var client = serviceClient.GetTableClient(TableNameFactory(typeof(T)));
            await client.CreateIfNotExistsAsync();
            return client;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
      
    }
    
    private static string TableNameFactory(Type type)
    {
        return type.Name switch
        {
            nameof(AnimeInfoStorage) => AzureTableMap.StoreTo.AnimeLibrary,
            nameof(AnimeInfoWithImageStorage) => AzureTableMap.StoreTo.AnimeLibrary,
            nameof(UpdateFeedAnimeInfoStorage) => AzureTableMap.StoreTo.AnimeLibrary,
            nameof(AlternativeTitleStorage) =>  AzureTableMap.StoreTo.AlternativeTitles,
            nameof(SubscriptionStorage) => AzureTableMap.StoreTo.Subscriptions,
            nameof(SeasonStorage) => AzureTableMap.StoreTo.AvailableSeasons,
            nameof(InterestedStorage) => AzureTableMap.StoreTo.InterestedSeries,
            nameof(TitlesStorage) => AzureTableMap.StoreTo.FeedTitles,
            nameof(ProcessedTitlesStorage) => AzureTableMap.StoreTo.ProcessedTitles,   
            nameof(UserStorage) => AzureTableMap.StoreTo.Users,
            nameof(OvaStorage) => AzureTableMap.StoreTo.OvaLibrary,
            nameof(OvasSubscriptionStorage) => AzureTableMap.StoreTo.OvaSubscriptions,
            nameof(MovieStorage) => AzureTableMap.StoreTo.MovieLibrary,
            nameof(MoviesSubscriptionStorage) => AzureTableMap.StoreTo.MovieSubscriptions,
            nameof(NotificationStorage) => AzureTableMap.StoreTo.Notifications,
            nameof(StateUpdateStorage) => AzureTableMap.StoreTo.StateUpdates,
            nameof(LatestSeasonsStorage) => AzureTableMap.StoreTo.JsonStorage,
            _ => throw new ArgumentException($"There is not a defined table for the type {type.FullName}")
        };
    }
}