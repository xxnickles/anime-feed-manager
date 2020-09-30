using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Services.Collectors.HorribleSubs;
using AnimeFeedManager.Services.Collectors.LiveChart;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AnimeFeedManager.DI
{
    public static class Registration
    {
        public static Func<IServiceProvider, Func<Type, string>> TableNameFactory => provider => type =>
        {
            return type.Name switch
            {
                nameof(AnimeInfoStorage) => AzureTable.TableMap.AnimeLibrary,
                nameof(SubscriptionStorage) => AzureTable.TableMap.Subscriptions,
                nameof(SeasonStorage) => AzureTable.TableMap.AvailableSeasons,
                nameof(InterestedStorage) => AzureTable.TableMap.InterestedSeries,
                _ => throw new ArgumentException($"There is not a defined table for the type {type.FullName}"),
            };
        };

        public static IServiceCollection RegisterStorage(this IServiceCollection services, string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            
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
            services.AddScoped<IFeedTitlesProvider, FeedTitles>();
            services.AddScoped<IExternalLibraryProvider, LibraryProvider>();
            services.AddScoped<IFeedProvider, FeedProvider>();
            return services;
        }
    }
}
