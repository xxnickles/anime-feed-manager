using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series;

public class AlternativeTitleUpdater
{
    private readonly ITvSeriesStore _seriesStore;
    private readonly ITittlesGetter _titlesProvider;

    public AlternativeTitleUpdater(ITvSeriesStore seriesStore, ITittlesGetter titlesProvider)
    {
        _seriesStore = seriesStore;
        _titlesProvider = titlesProvider;
    }

    public Task<Either<DomainError, Unit>> AddAlternativeTitle(RowKey rowKey, PartitionKey key, string alternativeTitle,
        CancellationToken token)
    {
        return _titlesProvider.GetTitles(token)
            .BindAsync(titles => _seriesStore.Add(new AnimeInfoStorage
            {
                RowKey = rowKey,
                PartitionKey = key,
                AlternativeTitle = alternativeTitle,
                FeedTitle = Utils.TryGetFeedTitle(titles, alternativeTitle)
            }, token));
    }
}