using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series;

public class MarkSeriesAsCompletedHandler(
    IIncompleteSeriesProvider incompleteSeriesProvider,
    IDomainPostman domainPostman,
    ITvSeriesStore seriesStore,
    ILogger<MarkSeriesAsCompletedHandler> logger)
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
            .BindAsync(_ => SendAlternativeTitlesEvents(series, token));
    }

    private async Task<Either<DomainError, int>> SendAlternativeTitlesEvents(ImmutableList<AnimeInfoStorage> series,
        CancellationToken token)
    {
        var tasks = series.Where(s => s.Status == SeriesStatus.Completed)
            .Select(s =>
                domainPostman.SendMessage(
                    new CompleteAlternativeTitle(s.RowKey ?? string.Empty, s.PartitionKey ?? string.Empty), token));

        var results = await Task.WhenAll(tasks);
        return results.FlattenResults().Map(_ => series.Count);
    }
}