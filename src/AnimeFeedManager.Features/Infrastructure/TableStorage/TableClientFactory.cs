﻿using AnimeFeedManager.Features.State.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Infrastructure.TableStorage
{
    public sealed class TableClientFactory<T> : ITableClientFactory<T> where T : ITableEntity
    {
        private readonly TableServiceClient _serviceClient;

        public TableClientFactory(
            TableServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public async Task<Either<DomainError,TableClient>> GetClient()
        {
            try
            {
                var client = _serviceClient.GetTableClient(TableNameFactory(typeof(T)));
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
                // nameof(SubscriptionStorage) => AzureTable.TableMap.Subscriptions,
                // nameof(SeasonStorage) => AzureTable.TableMap.AvailableSeasons,
                // nameof(InterestedStorage) => AzureTable.TableMap.InterestedSeries,
                // nameof(TitlesStorage) => AzureTable.TableMap.FeedTitles,
                // nameof(ProcessedTitlesStorage) => AzureTable.TableMap.ProcessedTitles,   
                // nameof(UserStorage) => AzureTable.TableMap.Users,
                // nameof(OvaStorage) => AzureTable.TableMap.OvaLibrary,
                // nameof(OvasSubscriptionStorage) => AzureTable.TableMap.OvaSubscriptions,
                // nameof(MovieStorage) => AzureTable.TableMap.MovieLibrary,
                // nameof(MoviesSubscriptionStorage) => AzureTable.TableMap.MovieSubscriptions,
                // nameof(NotificationStorage) => AzureTable.TableMap.Notifications,
                nameof(StateUpdateStorage) => AzureTableMap.StoreTo.StateUpdates,
                _ => throw new ArgumentException($"There is not a defined table for the type {type.FullName}")
            };
        }
    }
}