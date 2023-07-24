using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles;

public readonly record struct UpdateSeasonTitles (ImmutableList<string> Titles): INotification;

public sealed class UpdateSeasonTitlesHandler : INotificationHandler<UpdateSeasonTitles>
{
    private readonly ITitlesStore _titlesStore;
    private readonly ILogger<UpdateSeasonTitlesHandler> _logger;

    public UpdateSeasonTitlesHandler(ITitlesStore titlesStore, ILogger<UpdateSeasonTitlesHandler> logger)
    {
        _titlesStore = titlesStore;
        _logger = logger;
    }
    public async Task Handle(UpdateSeasonTitles notification, CancellationToken cancellationToken)
    {
        
       var result = await _titlesStore.UpdateTitles(notification.Titles, cancellationToken);

       result.Match(
           _ => _logger.LogInformation("Titles ({Count}) have been updated successfully", notification.Titles.Count),
           e => e.LogDomainError(_logger));
    }
}