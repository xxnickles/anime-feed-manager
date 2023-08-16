using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using MediatR;
using Microsoft.Extensions.Logging;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles;

public readonly record struct UpdateSeasonTitles(ImmutableList<string> Titles) : INotification;

public sealed class UpdateSeasonTitlesHandler : INotificationHandler<UpdateSeasonTitles>
{
    private readonly ITitlesStore _titlesStore;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<UpdateSeasonTitlesHandler> _logger;

    public UpdateSeasonTitlesHandler(
        ITitlesStore titlesStore,
        IDomainPostman domainPostman,
        ILogger<UpdateSeasonTitlesHandler> logger)
    {
        _titlesStore = titlesStore;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    public async Task Handle(UpdateSeasonTitles notification, CancellationToken cancellationToken)
    {
        var result = await _titlesStore.UpdateTitles(notification.Titles, cancellationToken)
            .BindAsync(_ => SendNotification(cancellationToken));

        result.Match(
            _ => _logger.LogInformation("Titles ({Count}) have been updated successfully", notification.Titles.Count),
            e => e.LogDomainError(_logger));
    }

    private async Task<Either<DomainError, Unit>> SendNotification(CancellationToken cancellationToken)
    {
        try
        {
            await _domainPostman.SendMessage(new TitlesUpdateNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Information,
                    "Latest feed titles have been updated"
                ), Boxes.TitleUpdatesNotifications,
                cancellationToken);

            return unit;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}