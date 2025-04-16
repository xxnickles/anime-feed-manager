﻿using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Notifications.Types;

namespace AnimeFeedManager.Features.Notifications.IO;

public interface IGetNotifications
{
    Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForUser(string userId, CancellationToken token);

    Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForAdmin(string userId, CancellationToken token);

    Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetFeedNotifications(string userId,
        CancellationToken token);
}

public class GetNotifications(ITableClientFactory<NotificationStorage> tableClientFactory)
    : IGetNotifications
{
    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForUser(string userId,
        CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteLimitedQuery(() =>
                client.QueryAsync<NotificationStorage>(n => n.PartitionKey == userId, cancellationToken: token)));
    }

    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForAdmin(string userId,
        CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteLimitedQuery(() =>
                client.QueryAsync<NotificationStorage>(
                    n => n.PartitionKey == userId || n.PartitionKey == RoleNames.Admin, cancellationToken: token)));
    }

    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetFeedNotifications(string userId,
        CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<NotificationStorage>(
                    n => n.PartitionKey == userId && n.Type == NotificationArea.Feed && n.For == NotificationTarget.Tv,
                    cancellationToken: token)));
    }
}