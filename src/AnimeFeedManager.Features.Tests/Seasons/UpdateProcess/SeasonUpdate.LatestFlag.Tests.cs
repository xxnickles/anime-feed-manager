using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.Storage.Stores;
using AnimeFeedManager.Features.Seasons.UpdateProcess;

namespace AnimeFeedManager.Features.Tests.Seasons.UpdateProcess;

public class SeasonUpdateLatestFlagTests
{
    [Fact]
    public async Task New_NonLatest_Should_Have_Latest_False_And_Partition_Season()
    {
        var incoming = new SeriesSeason(Season.Spring(), Year.FromNumber(2025), false);

        SeasonStorage? stored = null;

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(SeasonGetter, incoming, token)
            .CreateNewSeason()
            .AddLatestSeasonData(LatestGetter, token)
            .StoreUpdatedSeason(SeasonUpdater, token);

        result.AssertSuccess();
        Assert.NotNull(stored);
        Assert.False(stored.Latest);
        Assert.Equal(incoming.Season.ToString(), stored.Season);
        Assert.Equal(incoming.Year.Value, (ushort) stored.Year);
        return;

        Task<Result<SeasonStorageData>> SeasonGetter(SeriesSeason s, CancellationToken ct) =>
            Task.FromResult<Result<SeasonStorageData>>(new NoMatch());

        Task<Result<SeasonStorageData>> LatestGetter(CancellationToken ct) =>
            Task.FromResult<Result<SeasonStorageData>>(new CurrentLatestSeason(new SeasonStorage
            {
                PartitionKey = SeasonStorage.SeasonPartition,
                RowKey = "latest-2026-winter",
                Latest = true,
                Season = Season.Winter(),
                Year = 2026
            }));

        Task<Result<Unit>> SeasonUpdater(SeasonStorage s, CancellationToken ct)
        {
            stored = s;
            return Task.FromResult(Result<Unit>.Success());
        }
    }

    [Fact]
    public async Task New_Latest_Should_Set_Latest_True_And_Demote_Previous_To_False()
    {
        var incoming = new SeriesSeason(Season.Summer(), Year.FromNumber(2025), true);

        var previousLatest = new SeasonStorage
        {
            PartitionKey = SeasonStorage.SeasonPartition,
            RowKey = "latest-2025-spring",
            Latest = true,
            Season = Season.Spring(),
            Year = 2025
        };

        var calls = new List<SeasonStorage>();
        SeasonUpdater seasonUpdater = (s, _) =>
        {
            calls.Add(Clone(s));
            return Task.FromResult(Result<Unit>.Success());
        };

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(SeasonGetter, incoming, token)
            .CreateNewSeason()
            .AddLatestSeasonData(LatestGetter, token)
            .StoreUpdatedSeason(seasonUpdater, token)
            .DemoteCurrentLatest(seasonUpdater, token);

        result.AssertSuccess();
        Assert.Equal(2, calls.Count);
        var storedNewLatest = calls[0];
        var demoted = calls[1];

        Assert.True(storedNewLatest.Latest);
        Assert.Equal(incoming.Season.ToString(), storedNewLatest.Season);
        Assert.Equal(incoming.Year.Value, (ushort) storedNewLatest.Year);

        Assert.False(demoted.Latest);
        Assert.Equal(Season.Spring(), demoted.Season);
        Assert.Equal(2025, demoted.Year);
        return;

        Task<Result<SeasonStorageData>> SeasonGetter(SeriesSeason s, CancellationToken ct) =>
            Task.FromResult<Result<SeasonStorageData>>(new NoMatch());

        Task<Result<SeasonStorageData>> LatestGetter(CancellationToken ct) =>
            Task.FromResult<Result<SeasonStorageData>>(new CurrentLatestSeason(previousLatest));
    }

    [Fact]
    public async Task Updating_Current_Latest_Should_Keep_Latest_True_And_No_Demotion()
    {
        var currentLatest = new SeasonStorage
        {
            PartitionKey = SeasonStorage.SeasonPartition,
            RowKey = "latest-2026-fall",
            Latest = true,
            Season = Season.Fall(),
            Year = 2026
        };
        var incoming = new SeriesSeason(Season.Fall(), Year.FromNumber(2026), true);

        var latestGetter = A.Fake<LatestSeasonGetter>();

        var calls = 0;
        SeasonUpdater seasonUpdater = (s, _) =>
        {
            calls++;
            return Task.FromResult(Result<Unit>.Success());
        };

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(SeasonGetter, incoming, token)
            .CreateNewSeason() // no-op for existent current latest
            .AddLatestSeasonData(latestGetter, token) // predicate prevents call due to CurrentLatestSeason
            .StoreUpdatedSeason(seasonUpdater, token)
            .DemoteCurrentLatest(seasonUpdater, token);

        result.AssertSuccess();
        Assert.Equal(0, calls); // No updates nor demotions should be performed
        // Ensure latestGetter and demotion not invoked
        A.CallTo(() => latestGetter(A<CancellationToken>._)).MustNotHaveHappened();
        return;

        // matching the current latest will produce no update
        Task<Result<SeasonStorageData>> SeasonGetter(SeriesSeason s, CancellationToken ct) =>
            Task.FromResult<Result<SeasonStorageData>>(new NoUpdateRequired());
    }

    [Fact]
    public async Task Promote_Existent_NonLatest_To_Latest_Should_Set_True_And_Demote_Previous()
    {
        var incoming = new SeriesSeason(Season.Winter(), Year.FromNumber(2025), true);

        var existentNonLatest = new SeasonStorage
        {
            PartitionKey = SeasonStorage.SeasonPartition,
            RowKey = "season-2025-winter",
            Latest = false,
            Season = Season.Winter(),
            Year = 2025
        };

        var previousLatest = new SeasonStorage
        {
            PartitionKey = SeasonStorage.SeasonPartition,
            RowKey = "latest-2025-fall",
            Latest = true,
            Season = Season.Fall(),
            Year = 2025
        };

        var calls = new List<SeasonStorage>();
        SeasonUpdater seasonUpdater = (s, _) =>
        {
            calls.Add(Clone(s));
            return Task.FromResult(Result<Unit>.Success());
        };

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(SeasonGetter, incoming, token)
            .CreateNewSeason() // no-op because SeasonData is ReplaceLatestSeason already
            .AddLatestSeasonData(LatestGetter, token)
            .StoreUpdatedSeason(seasonUpdater, token)
            .DemoteCurrentLatest(seasonUpdater, token);

        result.AssertSuccess();
        Assert.Equal(2, calls.Count);
        var storedPromoted = calls[0];
        var demoted = calls[1];

        Assert.True(storedPromoted.Latest);
        Assert.Equal(SeasonStorage.SeasonPartition,
            storedPromoted.PartitionKey); // Note: stays in Season partition until other processes move it if needed
        Assert.Equal(incoming.Season.Value, storedPromoted.Season);
        Assert.Equal(incoming.Year.Value, (ushort) storedPromoted.Year);

        Assert.False(demoted.Latest);
        return;

        Task<Result<SeasonStorageData>> SeasonGetter(SeriesSeason s, CancellationToken ct) =>
            Task.FromResult<Result<SeasonStorageData>>(new ReplaceLatestSeason(new SeasonStorage
            {
                PartitionKey = SeasonStorage.SeasonPartition,
                RowKey = existentNonLatest.RowKey,
                Latest = true,
                Season = existentNonLatest.Season,
                Year = existentNonLatest.Year
            }));

        Task<Result<SeasonStorageData>> LatestGetter(CancellationToken ct) =>
            Task.FromResult<Result<SeasonStorageData>>(new CurrentLatestSeason(previousLatest));
    }

    // As SeasonStorage is mutable, we can clone it to avoid side effects in the original object
    private static SeasonStorage Clone(SeasonStorage s) => new()
    {
        Season = s.Season,
        Year = s.Year,
        PartitionKey = s.PartitionKey,
        Latest = s.Latest,
        RowKey = s.RowKey,
        Timestamp = s.Timestamp,
        ETag = s.ETag
    };
}