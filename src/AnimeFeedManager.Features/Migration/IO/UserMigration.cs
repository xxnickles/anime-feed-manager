using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;
using AnimeFeedManager.Features.Notifications.Types;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Migration.IO;

public class UserMigration(
    ITableClientFactory<UserStorage> userTableClientFactory,
    ITableClientFactory<SubscriptionStorage> subscriptionTableClientFactory,
    ITableClientFactory<MoviesSubscriptionStorage> moviesSubscriptionTableClientFactory,
    ITableClientFactory<OvasSubscriptionStorage> ovasSubscriptionTableClientFactory,
    ITableClientFactory<NotificationStorage> notificationTableClientFactory)
{
    private readonly ITableClientFactory<MoviesSubscriptionStorage> _moviesSubscriptionTableClientFactory = moviesSubscriptionTableClientFactory;

    private record SimpleUser(string Id, string Email);


    public Task<Either<DomainError, Unit>> MigrateUserData(CancellationToken token)
    {
        return userTableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<UserStorage>(cancellationToken: token)))
            .MapAsync(items =>
                items.ConvertAll(item => new SimpleUser(item.RowKey ?? string.Empty, item.Email ?? string.Empty)))
            .BindAsync(users => Process(users, token))
            .MapAsync(_ => unit);
    }

    private async Task<Either<DomainError, ImmutableList<Unit>>> Process(ImmutableList<SimpleUser> users,
        CancellationToken token)
    {
        var subsTask = users.AsParallel()
            .Select(user => ProcessSubscriptions(user, token)
            ).ToArray();

        var subsResults = await Task.WhenAll(subsTask);

        var ovasTask = users.AsParallel()
            .Select(user => ProcessOvasSubscriptions(user, token)
            ).ToArray();

        var ovasResults = await Task.WhenAll(ovasTask);

        var moviesTaks = users.AsParallel()
            .Select(user => ProcessMovieSubscriptions(user, token)
            ).ToArray();

        var moviesResults = await Task.WhenAll(moviesTaks);

        var notificationTasks = users.AsParallel()
            .Select(user => ProcessNotifications(user, token)
            ).ToArray();

        var notificationResults = await Task.WhenAll(notificationTasks);


        return subsResults.Flatten()
            .Bind(_ => ovasResults.Flatten())
            .Bind(_ => moviesResults.Flatten())
            .Bind(_ => notificationResults.Flatten());
    }


    private Task<Either<DomainError, Unit>> ProcessSubscriptions(SimpleUser user, CancellationToken token)
    {
        return subscriptionTableClientFactory.GetClient()
            .BindAsync(client => ProcessSubscriptions(client, user, token));
    }


    private Task<Either<DomainError, Unit>> ProcessSubscriptions(TableClient client, SimpleUser user,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == user.Email, cancellationToken: token))
            .BindAsync(items =>
                TableUtils.BatchDelete(client, items, token)
                    .BindAsync(_ => TableUtils.BatchAdd(client, items.ConvertAll(i =>
                    {
                        i.PartitionKey = user.Id;
                        return i;
                    }), token)));
    }


    private Task<Either<DomainError, Unit>> ProcessOvasSubscriptions(SimpleUser user, CancellationToken token)
    {
        return ovasSubscriptionTableClientFactory.GetClient()
            .BindAsync(client => ProcessOvasSubscriptions(client, user, token));
    }


    private Task<Either<DomainError, Unit>> ProcessOvasSubscriptions(TableClient client, SimpleUser user,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<OvasSubscriptionStorage>(s => s.PartitionKey == user.Email, cancellationToken: token))
            .BindAsync(items =>
                TableUtils.BatchDelete(client, items, token)
                    .BindAsync(_ => TableUtils.BatchAdd(client, items.ConvertAll(i =>
                    {
                        i.PartitionKey = user.Id;
                        return i;
                    }), token)));
    }

    private Task<Either<DomainError, Unit>> ProcessMovieSubscriptions(SimpleUser user, CancellationToken token)
    {
        return ovasSubscriptionTableClientFactory.GetClient()
            .BindAsync(client => ProcessMovieSubscriptions(client, user, token));
    }


    private Task<Either<DomainError, Unit>> ProcessMovieSubscriptions(TableClient client, SimpleUser user,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<MoviesSubscriptionStorage>(s => s.PartitionKey == user.Email,
                    cancellationToken: token))
            .BindAsync(items =>
                TableUtils.BatchDelete(client, items, token)
                    .BindAsync(_ => TableUtils.BatchAdd(client, items.ConvertAll(i =>
                    {
                        i.PartitionKey = user.Id;
                        return i;
                    }), token)));
    }


    private Task<Either<DomainError, Unit>> ProcessNotifications(SimpleUser user, CancellationToken token)
    {
        return notificationTableClientFactory.GetClient()
            .BindAsync(client => ProcessNotifications(client, user, token));
    }

    private Task<Either<DomainError, Unit>> ProcessNotifications(TableClient client, SimpleUser user,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<NotificationStorage>(s => s.PartitionKey == user.Email, cancellationToken: token))
            .BindAsync(items =>
                TableUtils.BatchDelete(client, items, token)
                    .BindAsync(_ => TableUtils.BatchAdd(client, items.ConvertAll(i =>
                    {
                        i.PartitionKey = user.Id;
                        return i;
                    }), token)));
    }
}