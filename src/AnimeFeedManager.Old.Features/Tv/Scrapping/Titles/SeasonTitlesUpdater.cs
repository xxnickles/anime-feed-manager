﻿using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles;

public sealed class SeasonTitlesUpdater(
    ITitlesStore titlesStore,
    IDomainPostman domainPostman)
{
    public Task<Either<DomainError, Unit>> Process(UpdateSeasonTitlesRequest notification,
        CancellationToken cancellationToken)
    {
        return titlesStore.UpdateTitles(notification.Titles, cancellationToken)
            .BindAsync(_ => SendNotification(cancellationToken))
            .BindAsync(_ => domainPostman.SendMessage(new MarkSeriesAsComplete(notification.Titles), cancellationToken))
            .BindAsync(_ => domainPostman.SendMessage(new AutomatedSubscription(), cancellationToken))
            .MapAsync(_ => unit);
    }

    private Task<Either<DomainError, Unit>> SendNotification(CancellationToken cancellationToken)
    {
        return domainPostman.SendMessage(new TitlesUpdateNotification(
                TargetAudience.Admins,
                NotificationType.Information,
                "Latest feed titles have been updated"
            ),
            cancellationToken);
    }
}