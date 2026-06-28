using System.Diagnostics;
using System.Net;
using AnimeFeedManager.Features.Auth.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Concurrency;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Auth.Storage;

/// <summary>
/// Registry handlers over the single <see cref="UsersIndex"/> document (system container).
/// Stream-based via <see cref="AuthJsonContext"/>, so the store is independent of the
/// globally-configured Cosmos serializer. Writes go through ETag optimistic concurrency.
/// </summary>
public static class CosmosUsersIndex
{
    private static readonly ActivitySource Source = new(Telemetry.AuthSource);

    public static UserByEmailGetter UserByEmailGetterHandler(this ICosmosContainerFactory factory) =>
        (email, cancellationToken) => factory.GetContainer<UsersIndex>()
            .Bind(container => LoadIndex(container, cancellationToken))
            .Map(index => FindByEmail(index, email));

    public static UsersIndexRegistrar UsersIndexRegistrarHandler(this ICosmosContainerFactory factory) =>
        (entry, cancellationToken) => factory.GetContainer<UsersIndex>()
            .Bind(container => Register(container, entry, cancellationToken));

    private static async Task<Result<UsersIndex>> LoadIndex(Container container, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Auth.UsersIndex.Read");
        var partitionKey = new PartitionKey(SystemDocument.SystemPartitionKey);
        try
        {
            using var response = await container.ReadItemStreamAsync(
                UsersIndex.DocumentId, partitionKey, cancellationToken: cancellationToken);
            activity?.SetTag("auth.users_index.cost.ru", Math.Round(response.Headers.RequestCharge, 2));

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new UsersIndex { Id = UsersIndex.DocumentId };

            response.EnsureSuccessStatusCode();
            var index = await JsonSerializer.DeserializeAsync(
                            response.Content, AuthJsonContext.Default.UsersIndex, cancellationToken)
                        ?? new UsersIndex { Id = UsersIndex.DocumentId };
            activity?.SetTag("auth.users_index.user_count", index.Users.Length);
            return index;
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, UsersIndex.DocumentId, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    // Load-merge-upsert guarded by ETag optimistic concurrency: a concurrent writer bumps the ETag,
    // so the conditioned upsert returns 412, which RegisterOnce surfaces as a retryable
    // CosmosResponseError and the policy reloads-and-retries. The brand-new (404) path has no ETag to
    // guard — a true first-write race could drop an entry, but registrations are rare and effectively
    // serial here, and every write once the doc exists is protected.
    private static async Task<Result<Unit>> Register(
        Container container,
        UserIndexEntry entry,
        CancellationToken cancellationToken)
    {
        // Span stays open across all retry attempts, so UntilWritten's retry-count tag lands here
        // (and nests under the surrounding registration-flow span once that exists). Per-attempt I/O
        // detail hangs on the child RegisterOnce spans.
        using var activity = Source.StartActivity("Auth.UsersIndex.Register");
        return await OptimisticConcurrency.UntilWritten(
            ct => RegisterOnce(container, entry, ct),
            cancellationToken,
            [HttpStatusCode.PreconditionFailed]);
    }

    private static async Task<Result<Unit>> RegisterOnce(
        Container container,
        UserIndexEntry entry,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Auth.UsersIndex.Register.Attempt");
        var partitionKey = new PartitionKey(SystemDocument.SystemPartitionKey);
        try
        {
            UsersIndex index;
            string? etag;
            double readRu;
            using (var read = await container.ReadItemStreamAsync(
                       UsersIndex.DocumentId, partitionKey, cancellationToken: cancellationToken))
            {
                readRu = read.Headers.RequestCharge;
                if (read.StatusCode == HttpStatusCode.NotFound)
                {
                    index = new UsersIndex { Id = UsersIndex.DocumentId };
                    etag = null;
                }
                else
                {
                    read.EnsureSuccessStatusCode();
                    index = await JsonSerializer.DeserializeAsync(
                                read.Content, AuthJsonContext.Default.UsersIndex, cancellationToken)
                            ?? new UsersIndex { Id = UsersIndex.DocumentId };
                    etag = read.Headers.ETag;
                }
            }

            var merged = index with { Users = Merge(index.Users, entry) };

            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, merged, AuthJsonContext.Default.UsersIndex, cancellationToken);
            stream.Position = 0;

            var options = etag is null ? null : new ItemRequestOptions { IfMatchEtag = etag };
            using var write = await container.UpsertItemStreamAsync(
                stream, partitionKey, options, cancellationToken);

            activity?.SetTag("auth.users_index.read.ru", Math.Round(readRu, 2));
            activity?.SetTag("auth.users_index.write.ru", Math.Round(write.Headers.RequestCharge, 2));

            if (write.IsSuccessStatusCode)
                return new Unit();

            // A 412 here rides the error channel as a retryable CosmosResponseError for the policy.
            return CosmosResponseError.Create(
                new CosmosException(
                    message: $"Users index upsert failed with status {write.StatusCode} ({write.ErrorMessage})",
                    statusCode: write.StatusCode,
                    subStatusCode: 0,
                    activityId: write.Headers.ActivityId,
                    requestCharge: write.Headers.RequestCharge),
                partitionKey,
                UsersIndex.DocumentId,
                container.Id);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, UsersIndex.DocumentId, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static StoredUser FindByEmail(UsersIndex index, Email email) =>
        index.Users.FirstOrDefault(e => string.Equals(e.Email, email, StringComparison.OrdinalIgnoreCase)) is { } entry
            ? CosmosUserStore.ToStoredUser(entry.Email, entry.UserId, entry.Role)
            : new NotAStoredUser();

    // Replace by user id (idempotent re-write), else append. Mirrors the seasons-index merge style.
    internal static ImmutableArray<UserIndexEntry> Merge(ImmutableArray<UserIndexEntry> existing, UserIndexEntry incoming)
    {
        if (existing.IsDefaultOrEmpty)
            return [incoming];

        var match = existing.FirstOrDefault(e => e.UserId == incoming.UserId);
        return match is null ? existing.Add(incoming) : existing.Replace(match, incoming);
    }
}
