using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Seasons.IO;
using AnimeFeedManager.Features.Seasons.Types;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Seasons
{
    public readonly record struct AddSeasonNotification(string Season, int Year, bool IsLatest) : INotification;

    public sealed class AddSeasonHandler : INotificationHandler<AddSeasonNotification>
    {
        private readonly ISeasonStore _seasonStore;
        private readonly ILogger<AddSeasonHandler> _logger;

        public AddSeasonHandler(ISeasonStore seasonStore, ILogger<AddSeasonHandler> logger)
        {
            _seasonStore = seasonStore;
            _logger = logger;
        }

        public async Task Handle(AddSeasonNotification notification, CancellationToken cancellationToken)
        {
            var seasonType = notification.IsLatest ? SeasonType.Latest : SeasonType.Season;
            var result = await _seasonStore.AddSeason(new SeasonStorage
            {
                PartitionKey = seasonType,
                RowKey = $"{notification.Year}-{notification.Season}",
                Season = notification.Season,
                Year = notification.Year,
                Latest = seasonType.IsLatest()

            }, seasonType , cancellationToken);
        
            result.Match(
                _ => _logger.LogInformation("Entry for {Season} updated successfully", $"{notification.Year}-{notification.Season}"),
                e =>  e.LogDomainError(_logger));
        }
    }
}