using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Events;
using AnimeFeedManager.Features.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Subscriptions;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles;

public sealed class SeasonTitlesUpdater
{
    private readonly ITitlesStore _titlesStore;
    private readonly IDomainPostman _domainPostman;
    private readonly IMediator _mediator;

    public SeasonTitlesUpdater(
        ITitlesStore titlesStore,
        IDomainPostman domainPostman,
        IMediator mediator)
    {
        _titlesStore = titlesStore;
        _domainPostman = domainPostman;
        _mediator = mediator;
    }

    public Task<Either<DomainError, Unit>> Process(UpdateSeasonTitlesRequest notification,
        CancellationToken cancellationToken)
    {
        return _titlesStore.UpdateTitles(notification.Titles, cancellationToken)
            .BindAsync(_ => SendNotification(cancellationToken))
            .MapAsync(_ => _mediator.Publish(new MarkSeriesAsComplete(notification.Titles), cancellationToken))
            .MapAsync(_ => _mediator.Publish(new AutomatedSubscription(), cancellationToken))
            .MapAsync(_ => unit);
    }

    private Task<Either<DomainError, Unit>> SendNotification(CancellationToken cancellationToken)
    {
        return _domainPostman.SendMessage(new TitlesUpdateNotification(
                TargetAudience.Admins,
                NotificationType.Information,
                "Latest feed titles have been updated"
            ), Box.TitleUpdatesNotifications,
            cancellationToken);
    }
}