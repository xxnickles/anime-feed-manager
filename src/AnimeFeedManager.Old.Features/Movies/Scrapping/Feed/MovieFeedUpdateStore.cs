﻿using System.Text.Json;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.Feed;

public class MovieFeedUpdateStore
{
    private readonly IMoviesStorage _moviesStorage;

    public MovieFeedUpdateStore(IMoviesStorage moviesStorage)
    {
        _moviesStorage = moviesStorage;
    }

    public Task<Either<DomainError, MovieFeedScrapResult>> StoreFeedUpdates(MovieStorage storage,
        ImmutableList<SeriesFeedLinks> links, CancellationToken token)
    {
        return links.IsEmpty
            ? HandleEmptyLinks(storage, token)
            : ProcessLinks(storage, links, token);
    }

    private Task<Either<DomainError, MovieFeedScrapResult>> HandleEmptyLinks(MovieStorage storage, CancellationToken token)
    {
        storage.Status = ShortSeriesStatus.NotFeedFound;
        return _moviesStorage.Update(storage, token).MapAsync(_ => MovieFeedScrapResult.NotFound);
    }

    private Task<Either<DomainError, MovieFeedScrapResult>> ProcessLinks(MovieStorage storage,
        ImmutableList<SeriesFeedLinks> links, CancellationToken token)
    {
        storage.Status = ShortSeriesStatus.Processed;
        storage.FeedInfo = JsonSerializer.Serialize(links.ToArray(), SeriesFeedLinksContext.Default.SeriesFeedLinksArray);
        return _moviesStorage.Update(storage, token)
            .MapAsync(_ => MovieFeedScrapResult.FoundAndUpdated);
    }
}