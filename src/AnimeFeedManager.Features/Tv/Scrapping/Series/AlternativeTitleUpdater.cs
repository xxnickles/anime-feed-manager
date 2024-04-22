using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series;

public enum AlternativeTitleUpdateResult
{
    ProcessComplete,
    TitleAddedNotFeedFound
}

public class AlternativeTitleUpdater
{
    private readonly IAlternativeTitlesStore _alternativeTitlesStore;
    private readonly ITvSeriesUpdates _tvSeriesUpdates;
    private readonly ITittlesGetter _titlesProvider;
    private readonly IDomainPostman _domainPostman;

    public AlternativeTitleUpdater(
        IAlternativeTitlesStore alternativeTitlesStore,
        ITvSeriesUpdates tvSeriesUpdates,
        ITittlesGetter titlesProvider,
        IDomainPostman domainPostman)
    {
        _alternativeTitlesStore = alternativeTitlesStore;
        _tvSeriesUpdates = tvSeriesUpdates;
        _titlesProvider = titlesProvider;
        _domainPostman = domainPostman;
    }

    public Task<Either<DomainError, AlternativeTitleUpdateResult>> AddAlternativeTitle(RowKey rowKey, PartitionKey key,
        string originalTitle,
        string alternativeTitle,
        CancellationToken token)
    {
        return _alternativeTitlesStore.Add(new AlternativeTitleStorage
            {
                RowKey = rowKey,
                PartitionKey = key,
                AlternativeTitle = alternativeTitle,
                OriginalTitle = originalTitle
            }, token)
            .BindAsync(_ => _titlesProvider.GetTitles(token))
            .MapAsync(titles => Utils.TryGetFeedTitle(titles, alternativeTitle))
            .BindAsync(feedTitle => UpdateAnimeLibrary(feedTitle, rowKey, key, token));
    }

    private Task<Either<DomainError, AlternativeTitleUpdateResult>> UpdateAnimeLibrary(string feedTitle, RowKey rowKey,
        PartitionKey key, CancellationToken token)
    {
        if (string.IsNullOrEmpty(feedTitle))
            return Task.FromResult(
                Right<DomainError, AlternativeTitleUpdateResult>(AlternativeTitleUpdateResult.TitleAddedNotFeedFound));

        return _tvSeriesUpdates.Update(rowKey, key, feedTitle, token)
            .BindAsync(_ => _domainPostman.SendMessage(new AutomatedSubscription(), token))
            .MapAsync(_ => AlternativeTitleUpdateResult.ProcessComplete);
    }

}