using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Seasons.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Seasons;

public sealed class AddSeasonNotificationHandler(
    ISeasonStore seasonStore,
    IDomainPostman domainPostman,
    ILogger<AddSeasonNotification> logger)
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