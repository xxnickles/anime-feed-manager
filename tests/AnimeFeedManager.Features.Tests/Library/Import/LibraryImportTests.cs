using System.Collections.Concurrent;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Images;
using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Features.Library.Import.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Types;


namespace AnimeFeedManager.Features.Tests.Library.Import;

public class LibraryImportTests
{
    private static readonly SeriesSeason Spring2026 = new(Season.Spring(), Year.FromNumber(2026));

    #region Persist + Index

    [Fact]
    public async Task Should_Persist_And_Index_All_Series_When_Page_Has_Items()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1), Anime(2)))));

        var persisted = new ConcurrentBag<Series>();
        SeasonEntry? indexed = null;

        var result = await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(persisted), CapturingUpsert(e => indexed = e), StoreOk);

        Assert.False(result.IsFailure);
        Assert.Equal(2, persisted.Count);
        Assert.NotNull(indexed);
        Assert.Equal(2, indexed!.SeriesCount);
    }

    [Fact]
    public async Task Should_Report_Zero_Imported_When_Page_Is_Empty()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page())));

        var persisted = new ConcurrentBag<Series>();
        SeasonEntry? indexed = null;

        var result = await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(persisted), CapturingUpsert(e => indexed = e), StoreOk);

        Assert.False(result.IsFailure);
        Assert.Empty(persisted);
        Assert.NotNull(indexed);
        Assert.Equal(0, indexed!.SeriesCount);
    }

    #endregion

    #region Cover (set-at-persist, best-effort)

    [Fact]
    public async Task Should_Persist_Cover_As_The_Stored_Blob_Path()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1, coverUrl: "https://cdn.example/cover.jpg")))));

        var persisted = new ConcurrentBag<Series>();

        await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(persisted), UpsertNoop, StoreReturns("images/2026/spring/1.jpg"));

        Assert.Equal("images/2026/spring/1.jpg", Assert.Single(persisted).CoverImageUrl);
    }

    [Fact]
    public async Task Should_Keep_Source_Cover_Url_When_Image_Storage_Fails()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1, coverUrl: "https://cdn.example/cover.jpg")))));

        var persisted = new ConcurrentBag<Series>();

        var result = await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(persisted), UpsertNoop, StoreFails);

        // Best-effort: the cover failure must not fail the series persist.
        Assert.False(result.IsFailure);
        Assert.Equal("https://cdn.example/cover.jpg", Assert.Single(persisted).CoverImageUrl);
    }

    #endregion

    #region Dedup + Skip

    [Fact]
    public async Task Should_Persist_Once_When_A_Series_Is_Listed_Twice_On_A_Page()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1), Anime(1)))));

        var persisted = new ConcurrentBag<Series>();

        await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(persisted), UpsertNoop, StoreOk);

        Assert.Single(persisted);
    }

    [Fact]
    public async Task Should_Skip_Item_When_Type_Is_Unknown()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1), Anime(2, type: "Music")))));

        var persisted = new ConcurrentBag<Series>();

        var result = await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(persisted), UpsertNoop, StoreOk);

        // The unmapped item is skipped (permanent), not retried — the import still succeeds.
        Assert.False(result.IsFailure);
        Assert.Equal("1", Assert.Single(persisted).Id);
    }

    #endregion

    #region Season routing

    [Fact]
    public async Task Should_Fetch_Specific_Season_When_Target_Is_Specific()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetSeason(Arg.Any<Year>(), Arg.Any<Season>(), Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1)))));

        var result = await RunImport(ImportTarget.For(Spring2026), jikan,
            RecordingPersist(new ConcurrentBag<Series>()), UpsertNoop, StoreOk);

        Assert.False(result.IsFailure);
        jikan.Received(1).GetSeason(Arg.Any<Year>(), Arg.Any<Season>(), Arg.Any<CancellationToken>());
        jikan.DidNotReceive().GetCurrentSeason(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Fetch_Current_Season_When_Target_Is_Current()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1)))));

        var result = await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(new ConcurrentBag<Series>()), UpsertNoop, StoreOk);

        Assert.False(result.IsFailure);
        jikan.Received(1).GetCurrentSeason(Arg.Any<CancellationToken>());
        jikan.DidNotReceive().GetSeason(Arg.Any<Year>(), Arg.Any<Season>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Seasons index — kind + poster

    [Fact]
    public async Task Should_Pass_Current_Kind_When_Target_Is_Current()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1)))));

        SeasonImportKind? kind = null;
        await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(new ConcurrentBag<Series>()), CapturingUpsert((_, k) => kind = k), StoreOk);

        Assert.Equal(SeasonImportKind.Current, kind);
    }

    [Fact]
    public async Task Should_Pass_Specific_Kind_When_Target_Is_Specific()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetSeason(Arg.Any<Year>(), Arg.Any<Season>(), Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1)))));

        SeasonImportKind? kind = null;
        await RunImport(ImportTarget.For(Spring2026), jikan,
            RecordingPersist(new ConcurrentBag<Series>()), CapturingUpsert((_, k) => kind = k), StoreOk);

        Assert.Equal(SeasonImportKind.Specific, kind);
    }

    [Fact]
    public async Task Should_Index_The_Highest_Scored_Stored_Cover_As_The_Representative_Poster()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(
                Anime(1, coverUrl: "https://cdn.example/a.jpg", score: 7.0),
                Anime(2, coverUrl: "https://cdn.example/b.jpg", score: 9.0)))));

        SeasonEntry? indexed = null;
        await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(new ConcurrentBag<Series>()), CapturingUpsert(e => indexed = e), StoreEchoesId);

        Assert.Equal("images/2.webp", indexed!.RepresentativePoster);
    }

    [Fact]
    public async Task Should_Index_An_Empty_Poster_When_No_Cover_Is_Stored()
    {
        var jikan = Substitute.For<IJikanClient>();
        jikan.GetCurrentSeason(Arg.Any<CancellationToken>())
            .Returns(Stream(Result<JikanPage>.Success(Page(Anime(1, coverUrl: "https://cdn.example/a.jpg")))));

        SeasonEntry? indexed = null;
        // Storage fails -> the series keeps its source URL but contributes no stored poster path.
        await RunImport(ImportTarget.Now(), jikan,
            RecordingPersist(new ConcurrentBag<Series>()), CapturingUpsert(e => indexed = e), StoreFails);

        Assert.Equal(string.Empty, indexed!.RepresentativePoster);
    }

    #endregion

    #region Test Helpers

    private static Task<Result<Unit>> RunImport(
        ImportTarget target,
        IJikanClient jikan,
        SingleSeriesPersistenceHandler<CosmosOperationCost> persistSeries,
        LibrarySeasonsIndexUpserter upsertIndex,
        SeriesImageProcessor processImage) =>
        LibraryImport.Execute(
            target, jikan, persistSeries, upsertIndex, processImage,
            TimeProvider.System, TestContext.Current.CancellationToken);

    private static SingleSeriesPersistenceHandler<CosmosOperationCost> RecordingPersist(ConcurrentBag<Series> into) =>
        (series, _) =>
        {
            into.Add(series);
            return Task.FromResult(Result<CosmosOperationCost>.Success(new CosmosOperationCost(1)));
        };

    private static LibrarySeasonsIndexUpserter CapturingUpsert(Action<SeasonEntry> capture) =>
        (entry, _, _) =>
        {
            capture(entry);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

    private static LibrarySeasonsIndexUpserter CapturingUpsert(Action<SeasonEntry, SeasonImportKind> capture) =>
        (entry, kind, _) =>
        {
            capture(entry, kind);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

    private static readonly LibrarySeasonsIndexUpserter UpsertNoop =
        (_, _, _) => Task.FromResult(Result<Unit>.Success(new Unit()));

    private static SeriesImageProcessor StoreReturns(string blobPath) =>
        (_, _, _, _) => Task.FromResult(Result<string>.Success(blobPath));

    // Echoes a per-series blob path so tests can assert which cover was chosen as the poster.
    private static readonly SeriesImageProcessor StoreEchoesId =
        (id, _, _, _) => Task.FromResult(Result<string>.Success($"images/{id}.webp"));

    private static readonly SeriesImageProcessor StoreOk =
        (_, _, _, _) => Task.FromResult(Result<string>.Success("images/2026/spring/blob.webp"));

    private static readonly SeriesImageProcessor StoreFails =
        (_, _, _, _) =>
        {
            Result<string> failure = ExceptionError.FromException(new Exception("image storage failed"));
            return Task.FromResult(failure);
        };

    private static async IAsyncEnumerable<Result<JikanPage>> Stream(params Result<JikanPage>[] pages)
    {
        foreach (var page in pages)
            yield return page;
        await Task.CompletedTask;
    }

    private static JikanPage Page(params JikanAnime[] items) =>
        new([.. items], Page: 1, LastPage: 1, TotalItems: items.Length) { Season = Spring2026 };

    private static JikanAnime Anime(int malId, string? coverUrl = null, string type = "TV",
        string title = "Test Series", double? score = null)
    {
        var images = coverUrl is null
            ? null
            : new JikanImages(Jpg: null, Webp: new JikanImageVariants(ImageUrl: null, SmallImageUrl: null, LargeImageUrl: coverUrl));

        return new JikanAnime(
            MalId: malId, Url: null, Approved: true, Images: images, Trailer: null,
            Type: type, Source: null, Episodes: null,
            Status: null, Airing: false, Aired: null, Duration: null, Rating: null, Score: score,
            ScoredBy: null, Rank: null, Popularity: null, Members: null, Favorites: null,
            Synopsis: null, Background: null, Season: null, Year: null, Broadcast: null)
        {
            Titles = [new JikanTitle("Default", title)]
        };
    }

    #endregion
}
