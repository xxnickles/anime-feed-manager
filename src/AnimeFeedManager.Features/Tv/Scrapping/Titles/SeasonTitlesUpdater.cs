﻿using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Subscriptions;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles;

public sealed class SeasonTitlesUpdater(
    ITitlesStore titlesStore,
    IDomainPostman domainPostman,
    IMediator mediator)
{
    public Task<Either<DomainError, Unit>> Process(UpdateSeasonTitlesRequest notification,
        CancellationToken cancellationToken)
    {
        return titlesStore.UpdateTitles(notification.Titles, cancellationToken)
            .BindAsync(_ => SendNotification(cancellationToken))
            .MapAsync(_ => mediator.Publish(new MarkSeriesAsComplete(notification.Titles), cancellationToken))
            .MapAsync(_ => mediator.Publish(new AutomatedSubscription(), cancellationToken))
            .MapAsync(_ => unit);
    }

    private Task<Either<DomainError, Unit>> SendNotification(CancellationToken cancellationToken)
    {
        return domainPostman.SendMessage(new TitlesUpdateNotification(
                TargetAudience.Admins,
                NotificationType.Information,
                "Latest feed titles have been updated"
            ), Box.TitleUpdatesNotifications,
            cancellationToken);
    }
}