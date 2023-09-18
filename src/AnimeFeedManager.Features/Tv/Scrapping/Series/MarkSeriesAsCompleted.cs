using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Types;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series;

public readonly record struct MarkSeriesAsComplete(ImmutableList<string> Titles) : INotification;

public class MarkSeriesAsCompletedHandler : INotificationHandler<MarkSeriesAsComplete>
{
    private readonly IIncompleteSeriesProvider _incompleteSeriesProvider;
    private readonly ITvSeriesStore _seriesStore;
    private readonly ILogger<MarkSeriesAsCompletedHandler> _logger;

    public MarkSeriesAsCompletedHandler(
        IIncompleteSeriesProvider incompleteSeriesProvider,
        ITvSeriesStore seriesStore,
        ILogger<MarkSeriesAsCompletedHandler> logger)
    {
        _incompleteSeriesProvider = incompleteSeriesProvider;
        _seriesStore = seriesStore;
        _logger = logger;
    }

    public async Task Handle(MarkSeriesAsComplete notification, CancellationToken cancellationToken)
    {
        var result = await _incompleteSeriesProvider.GetIncompleteSeries(cancellationToken)
            .MapAsync(series =>
                series.Where(anime => anime.FeedTitle != null && !notification.Titles.Contains(anime.FeedTitle)))
            .MapAsync(series => series.Select(MarkAsCompleted))
            .BindAsync(series => Persist(series.ToImmutableList(), cancellationToken));

        result.Match(
            count => _logger.LogInformation("({Count}) series have been marked as completed", count),
            e => e.LogDomainError(_logger));
    }

    private static AnimeInfoStorage MarkAsCompleted(AnimeInfoStorage original)
    {
        original.Status = SeriesStatus.Completed;
        return original;
    }

    private Task<Either<DomainError, int>> Persist(ImmutableList<AnimeInfoStorage> series, CancellationToken token)
    {
        return _seriesStore.Add(series, token)
            .MapAsync(_ => series.Count);
    }
}