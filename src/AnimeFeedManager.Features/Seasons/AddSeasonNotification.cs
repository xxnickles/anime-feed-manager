using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Seasons.IO;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Seasons;

public readonly record struct AddSeasonNotification(string Season, int Year, bool IsLatest) : INotification;

public sealed class AddSeasonHandler(
    ISeasonStore seasonStore,
    IDomainPostman domainPostman,
    ILogger<AddSeasonHandler> logger)
    : INotificationHandler<AddSeasonNotification>
{
    public async Task Handle(AddSeasonNotification notification, CancellationToken cancellationToken)
    {
        var seasonType = notification.IsLatest ? SeasonType.Latest : SeasonType.Season;
        var result = await seasonStore.AddSeason(new SeasonStorage
            {
                PartitionKey = seasonType,
                RowKey = $"{notification.Year}-{notification.Season}",
                Season = notification.Season,
                Year = notification.Year,
                Latest = seasonType.IsLatest()
            }, seasonType, cancellationToken)
            .BindAsync(_ =>
                domainPostman.SendMessage(new 
                    UpdateLatestSeasonsRequest(), Box.LatestSeason, cancellationToken));

        result.Match(
            _ => logger.LogInformation("Entry for {Season} updated successfully",
                $"{notification.Year}-{notification.Season}"),
            e => e.LogError(logger));
    }
}