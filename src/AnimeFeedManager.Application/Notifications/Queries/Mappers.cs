using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Notifications;

namespace AnimeFeedManager.Application.Notifications.Queries;

internal sealed class Mappers
{
    internal static UiNotification Map(NotificationStorage storage)
    {
        return new UiNotification(
            storage.Type!,
            storage.Timestamp.GetValueOrDefault(),
            storage.Payload!);
    }

    internal static UiNotifications Map(ImmutableList<NotificationStorage> storage)
    {
        var tv = storage
            .Where(n => NotificationFor.Tv.Equals(n.For))
            .Select(Map)
            .ToArray();
        var ovas = storage
            .Where(n => NotificationFor.Ova.Equals(n.For))
            .Select(Map)
            .ToArray();
        var movies = storage
            .Where(n => NotificationFor.Movie.Equals(n.For))
            .Select(Map)
            .ToArray();
        var images = storage
            .Where(n => NotificationFor.Images.Equals(n.For))
            .Select(Map)
            .ToArray();
        var tvTitles = storage
            .Where(n => NotificationFor.TvTitles.Equals(n.For))
            .Select(Map)
            .ToArray();
        var admin = storage
            .Where(n => NotificationFor.Admin.Equals(n.For))
            .Select(Map)
            .ToArray();

        return new UiNotifications(
            tv,
            tvTitles,
            ovas,
            movies,
            images,
            admin
        );

    }
}