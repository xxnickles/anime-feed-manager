#pragma warning disable ASPIRECOSMOSDB001

using AnimeFeedManager.Features;
using AnimeFeedManager.Features.Auth;
using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Shared.Results.Static;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.AspireHost;

internal static class AdminSeedExtensions
{
    /// <summary>
    /// Dev-only admin seed wired into orchestration (not the app runtime). Once the emulator and its
    /// containers are provisioned (Aspire's own provisioning handler runs before this), it bridges an
    /// already-registered Passwordless user — <c>AdminSeed:Id</c> + <c>AdminSeed:Email</c>, set in the
    /// AppHost user-secrets — to a Cosmos admin account via <see cref="AdminSeed"/>. Config-gated no-op
    /// when unset; idempotent (account upsert + registry merge), so it is safe every start.
    /// </summary>
    public static IResourceBuilder<AzureCosmosDBResource> SeedAdminOnReady(
        this IResourceBuilder<AzureCosmosDBResource> cosmos,
        IConfiguration configuration,
        string databaseName)
    {
        cosmos.OnResourceReady(async (resource, evt, cancellationToken) =>
        {
            var seedId = configuration["AdminSeed:Id"];
            var seedEmail = configuration["AdminSeed:Email"];
            var logger = evt.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeed");

            if (string.IsNullOrWhiteSpace(seedId) || string.IsNullOrWhiteSpace(seedEmail))
            {
                logger.LogInformation("AdminSeed:Id / AdminSeed:Email not configured; skipping admin seed.");
                return;
            }

            var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogWarning("Cosmos connection string unavailable; skipping admin seed.");
                return;
            }

            using var client = new CosmosClient(connectionString, new CosmosClientOptions
            {
                // Emulator: gateway mode + single endpoint, and accept its self-signed dev cert (dev-only).
                ConnectionMode = ConnectionMode.Gateway,
                LimitToEndpoint = true,
                HttpClientFactory = () => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                })
            });

            var factory = new CosmosContainerFactory(client.GetDatabase(databaseName), CosmosContainerRegistry.EntityRegistry);

            var result = await AdminSeed.Run(factory, seedId, seedEmail, cancellationToken);
            result.Match(
                _ => logger.LogInformation("Seeded admin account for {Email} ({UserId}).", seedEmail, seedId),
                error => logger.LogError("Admin seed failed for {Email}: {Error}", seedEmail, error.Message));
        });

        return cosmos;
    }
}
