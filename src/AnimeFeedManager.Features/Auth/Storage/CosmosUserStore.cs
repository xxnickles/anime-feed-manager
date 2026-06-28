using System.Diagnostics;
using System.Net;
using AnimeFeedManager.Features.Auth.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Auth.Storage;

/// <summary>
/// Account read/write handlers for the <c>users</c> container, built as delegates over
/// <see cref="ICosmosContainerFactory"/> (same composition pattern as the library handlers).
/// Stream-based (de)serialization via <see cref="AuthJsonContext"/> so the account's polymorphic
/// <c>docType</c> discriminator round-trips correctly and the store doesn't depend on the
/// globally-configured Cosmos serializer.
/// </summary>
public static class CosmosUserStore
{
    private static readonly ActivitySource Source = new(Telemetry.AuthSource);

    public static UserAccountGetter UserAccountGetterHandler(this ICosmosContainerFactory factory) =>
        (userId, cancellationToken) => factory.GetContainer<UserAccount>()
            .Bind(container => LoadAccount(container, userId, cancellationToken));

    public static UserAccountUpserter UserAccountUpserterHandler(this ICosmosContainerFactory factory) =>
        (email, userId, role, displayName, cancellationToken) => factory.GetContainer<UserAccount>()
            .Bind(container => UpsertAccount(container, email, userId, role, displayName, cancellationToken));

    private static async Task<Result<StoredUser>> LoadAccount(
        Container container,
        NoEmptyString userId,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Auth.UserAccount.Read");
        var partitionKey = new PartitionKey(userId.ToString());
        try
        {
            using var response = await container.ReadItemStreamAsync(
                UserAccount.DocumentId, partitionKey, cancellationToken: cancellationToken);
            activity?.SetTag("auth.user_account.cost.ru", Math.Round(response.Headers.RequestCharge, 2));

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                activity?.SetTag("auth.user_account.found", false);
                return new NotAStoredUser();
            }

            response.EnsureSuccessStatusCode();

            var document = await JsonSerializer.DeserializeAsync(
                response.Content, AuthJsonContext.Default.UserDocument, cancellationToken);

            activity?.SetTag("auth.user_account.found", document is UserAccount);
            return document is UserAccount account
                ? ToStoredUser(account.Email, account.UserId, account.Role)
                : new NotAStoredUser();
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, UserAccount.DocumentId, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static async Task<Result<Unit>> UpsertAccount(
        Container container,
        Email email,
        string userId,
        UserRole role,
        string displayName,
        CancellationToken cancellationToken)
    {
        var account = new UserAccount
        {
            UserId = userId,
            Email = email,
            DisplayName = displayName,
            Role = role.ToString()
        };
        var partitionKey = new PartitionKey(userId);
        using var activity = Source.StartActivity("Auth.UserAccount.Upsert");
        try
        {
            // Serialize through the polymorphic base type info so the `docType` discriminator is
            // written; Cosmos never deserializes the response, so we just check the status.
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(
                stream, (UserDocument)account, AuthJsonContext.Default.UserDocument, cancellationToken);
            stream.Position = 0;

            using var response = await container.UpsertItemStreamAsync(
                stream, partitionKey, cancellationToken: cancellationToken);
            activity?.SetTag("auth.user_account.cost.ru", Math.Round(response.Headers.RequestCharge, 2));

            if (response.IsSuccessStatusCode)
                return new Unit();

            return CosmosResponseError.Create(
                new CosmosException(
                    message: $"User account upsert failed with status {response.StatusCode} ({response.ErrorMessage})",
                    statusCode: response.StatusCode,
                    subStatusCode: 0,
                    activityId: response.Headers.ActivityId,
                    requestCharge: response.Headers.RequestCharge),
                partitionKey,
                account.Id,
                container.Id);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, account.Id, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    // Parse the stored primitives back into domain types at the boundary; a malformed record
    // (should never happen — we only write validated values) degrades to NotAStoredUser.
    internal static StoredUser ToStoredUser(string email, string userId, string role) =>
        email.ParseAsEmail().AsResult()
            .Bind(parsedEmail => userId.ParseAsNonEmpty().AsResult().Map(parsedId => (parsedEmail, parsedId)))
            .MatchToValue<(Email, NoEmptyString), StoredUser>(
                parts => new ValidStoredUser(parts.Item1, parts.Item2, UserRole.FromString(role)),
                _ => new NotAStoredUser());
}
