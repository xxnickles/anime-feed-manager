using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Types;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series;

public readonly record struct MarkSeriesAsComplete(ImmutableList<string> Titles) : INotification;

public class MarkSeriesAsCompletedHandler(
    IIncompleteSeriesProvider incompleteSeriesProvider,
    ITvSeriesStore seriesStore,
    ILogger<MarkSeriesAsCompletedHandler> logger)
    : INotificationHandler<MarkSeriesAsComplete>
{
    public async Task Handle(MarkSeriesAsComplete notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting process to complete series in library");
        var result = await incompleteSeriesProvider.GetIncompleteSeries(cancellationToken)
            .MapAsync(series =>
                series.Where(anime => anime.FeedTitle != null && !notification.Titles.Contains(anime.FeedTitle)))
            .MapAsync(series => series.Select(MarkAsCompleted))
            .BindAsync(series => Persist(series.ToImmutableList(), cancellationToken));

        result.Match(
            count => logger.LogInformation("({Count}) series have been marked as completed", count),
            e => e.LogError(logger));
    }

    private static AnimeInfoStorage MarkAsCompleted(AnimeInfoStorage original)
    {
        original.Status = SeriesStatus.Completed;
        return original;
    }

    private Task<Either<DomainError, int>> Persist(ImmutableList<AnimeInfoStorage> series, CancellationToken token)
    {
        return seriesStore.Add(series, token)
            .MapAsync(_ => series.Count);
    }
}