using System;
using System.Reflection;
using AnimeFeedManager.Services.Collectors.HorribleSubs;
using AnimeFeedManager.Services.Collectors.LiveChart;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using AnimeFeedManager.Storage.Repositories;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddMediatR(Assembly.GetExecutingAssembly());
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
