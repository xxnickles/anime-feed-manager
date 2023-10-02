﻿using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Subscriptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles;

public readonly record struct UpdateSeasonTitles(ImmutableList<string> Titles) : INotification;

public sealed class UpdateSeasonTitlesHandler : INotificationHandler<UpdateSeasonTitles>
{
    private readonly ITitlesStore _titlesStore;
    private readonly IDomainPostman _domainPostman;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateSeasonTitlesHandler> _logger;

    public UpdateSeasonTitlesHandler(
        ITitlesStore titlesStore,
        IDomainPostman domainPostman,
        IMediator mediator,
        ILogger<UpdateSeasonTitlesHandler> logger)
    {
        _titlesStore = titlesStore;
        _domainPostman = domainPostman;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(UpdateSeasonTitles notification, CancellationToken cancellationToken)
    {
        var result = await _titlesStore.UpdateTitles(notification.Titles, cancellationToken)
            .BindAsync(_ => SendNotification(cancellationToken))
            .MapAsync(_ => _mediator.Publish(new MarkSeriesAsComplete(notification.Titles), cancellationToken))
            .MapAsync(_ => _mediator.Publish(new AutomatedSubscription(), cancellationToken))
            .MapAsync(_ => unit);

        result.Match(
            _ => _logger.LogInformation("Titles ({Count}) have been updated successfully", notification.Titles.Count.ToString()),
            e => e.LogDomainError(_logger));
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