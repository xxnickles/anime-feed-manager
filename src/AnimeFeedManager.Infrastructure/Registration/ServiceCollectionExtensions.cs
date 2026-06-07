using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AnimeFeedManager.Infrastructure.Background.Cron;
using AnimeFeedManager.Infrastructure.Background.Jobs;
using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Infrastructure.Eventing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimeFeedManager.Infrastructure.Registration;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers <see cref="EventBus"/> as a singleton. The bus's pump task starts
        /// when the singleton is first resolved.
        /// </summary>
        public IServiceCollection AddEventBus()
        {
            services.AddSingleton<EventBus>();
            return services;
        }

        /// <summary>
        /// Registers <typeparamref name="TJob"/> as a scoped <see cref="CronJob"/>.
        /// The job is resolvable both by its concrete type (for per-fire resolution)
        /// and as part of <see cref="IEnumerable{CronJob}"/> (for scheduler discovery).
        /// </summary>
        public IServiceCollection AddCronJob<TJob>() where TJob : CronJob
        {
            services.AddScoped<TJob>();
            services.AddScoped<CronJob>(sp => sp.GetRequiredService<TJob>());
            return services;
        }
    }

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers the cron scheduler infrastructure: <see cref="TimeProvider"/>,
        /// <c>"CronJobs"</c> configuration binding via <see cref="IOptionsMonitor{T}"/>,
        /// and the hosted scheduler service. Jobs are added separately via
        /// <c>AddCronJob&lt;TJob&gt;</c> and discovered at startup as
        /// <see cref="IEnumerable{CronJob}"/>. Configuration edits to
        /// <c>appsettings.json</c> take effect without restart.
        /// </summary>
        public IHostApplicationBuilder AddCronScheduler()
        {
            builder.Services.TryAddSingleton(TimeProvider.System);
            builder.Services.TryAddSingleton<JobExecutor>();
            builder.Services.AddOptions<CronJobsOptions>()
                .Bind(builder.Configuration.GetSection(CronJobsOptions.SectionName));
            builder.Services.AddHostedService<CronHostedService>();

            return builder;
        }

        /// <summary>
        /// Registers the full Cosmos DB infrastructure: client, database, serializer, and container factory.
        /// Reads <see cref="CosmosOptions"/> from configuration, builds a source-gen-aware
        /// <see cref="JsonSerializerOptions"/> with the provided <paramref name="jsonContexts"/> as resolvers,
        /// and wires up the Aspire Cosmos integration.
        /// </summary>
        public IHostApplicationBuilder AddCosmosInfrastructure(IReadOnlyDictionary<Type, ContainerInfo> entityRegistry,
            params JsonSerializerContext[] jsonContexts)
        {
            var cosmosOptions =
                builder.Configuration.GetSection(CosmosOptions.SectionName).Get<CosmosOptions>()
                ?? throw new InvalidOperationException(
                    $"Missing/invalid configuration section '{CosmosOptions.SectionName}'.");

            IJsonTypeInfoResolver[] resolvers = [.. jsonContexts, new DefaultJsonTypeInfoResolver()];

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                TypeInfoResolver = JsonTypeInfoResolver.Combine(resolvers)
            };

            builder.AddAzureCosmosDatabase(cosmosOptions.DatabaseName,
                configureSettings: settings =>
                {
                    settings.DatabaseName = cosmosOptions.DatabaseName;
                },
                configureClientOptions: opts =>
                {
                    opts.UseSystemTextJsonSerializerWithOptions = jsonOptions;
                    opts.AllowBulkExecution = cosmosOptions.AllowBulkExecution;
                });

            builder.AddCosmosContainerFactory(entityRegistry);
            builder.Services.Configure<CosmosOptions>(
                builder.Configuration.GetSection(CosmosOptions.SectionName));

            return builder;
        }

        /// <summary>
        /// Registers <see cref="ICosmosContainerFactory"/> using the Aspire-provided <see cref="Database"/> singleton.
        /// Call after <c>AddAzureCosmosDatabase</c> which registers <see cref="CosmosClient"/> and <see cref="Database"/>.
        /// </summary>
        private IHostApplicationBuilder AddCosmosContainerFactory(IReadOnlyDictionary<Type, ContainerInfo> entityRegistry)
        {
            builder.Services.AddSingleton<ICosmosContainerFactory>(sp =>
                new CosmosContainerFactory(sp.GetRequiredService<Database>(), entityRegistry));

            return builder;
        }
    }
}
