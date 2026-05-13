using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AnimeFeedManager.Infrastructure.Cosmos;

namespace AnimeFeedManager.Infrastructure.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
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
