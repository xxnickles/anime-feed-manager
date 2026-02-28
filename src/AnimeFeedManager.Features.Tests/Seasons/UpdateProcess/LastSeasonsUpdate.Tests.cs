using System.Text.Json;
using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using CommonJsonContext = AnimeFeedManager.Features.Common.CommonJsonContext;

namespace AnimeFeedManager.Features.Tests.Seasons.UpdateProcess;

public class LastSeasonsUpdateTests
{
    [Fact]
    public async Task Should_Not_Update_When_No_Changes()
    {
        var incoming = new SeriesSeason(Season.Spring(), Year.FromNumber(2025));

        var token = CancellationToken.None;
        var latestUpdaterCalled = false;

        var result = await SeasonUpdate
            .CheckSeasonExist(SeasonGetter, incoming, token)
            .UpdateLast4Seasons(GetAllSeasons, LatestUpdater, token);

        Assert.False(latestUpdaterCalled); // No update should occur
        return;

        // SeasonGetter returns NoUpdateRequired to simulate no change
        Task<Result<SeasonStorageData>> SeasonGetter(SeriesSeason _, CancellationToken __) =>
            Task.FromResult<Result<SeasonStorageData>>(new NoUpdateRequired());

        Task<Result<Unit>> LatestUpdater(LatestSeasonsStorage s, CancellationToken _)
        {
            latestUpdaterCalled = true;
            return Task.FromResult(Result<Unit>.Success());
        }

        Task<Result<ImmutableArray<SeriesSeason>>> GetAllSeasons(CancellationToken _) =>
            Task.FromResult(Result<ImmutableArray<SeriesSeason>>.Success(ImmutableArray<SeriesSeason>.Empty));
    }

    [Fact]
    public async Task Should_Update_With_Last_4_Seasons_In_Order()
    {
        var incoming = new SeriesSeason(Season.Summer(), Year.FromNumber(2026), true);
        // Force path that is not NoUpdateRequired so UpdateLast4Seasons executes
        Task<Result<SeasonStorageData>> SeasonGetter(SeriesSeason _, CancellationToken __) =>
            Task.FromResult<Result<SeasonStorageData>>(new NewSeason(new SeasonStorage()));

        var seasons = new List<SeriesSeason>
        {
            new(Season.Winter(), Year.FromNumber(2024)),
            new(Season.Spring(), Year.FromNumber(2025)),
            new(Season.Summer(), Year.FromNumber(2025), true),
            new(Season.Fall(), Year.FromNumber(2025)),
            new(Season.Winter(), Year.FromNumber(2025)),
            new(Season.Spring(), Year.FromNumber(2026)),
            new(Season.Summer(), Year.FromNumber(2026), true)
        };

        LatestSeasonsStorage? stored = null;

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(SeasonGetter, incoming, token)
            .UpdateLast4Seasons(GetAllSeasons, LatestUpdater, token);

        Assert.NotNull(stored);
        // Deserialize and compare with algorithm used in production (OrderByDescending Year, ThenByDescending Season, Take 4, Reverse)
        var parsed = JsonSerializer.Deserialize(stored.Payload ?? string.Empty, CommonJsonContext.Default.SeriesSeasonArray) ?? [];
        Assert.Equal(4, parsed.Length);
        var expected = seasons
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Season)
            .Take(4)
            .Reverse()
            .ToArray();
        Assert.Equal(expected.Length, parsed.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Season, parsed[i].Season);
            Assert.Equal(expected[i].Year, parsed[i].Year);
        }
        return;

        Task<Result<Unit>> LatestUpdater(LatestSeasonsStorage s, CancellationToken _)
        {
            stored = s;
            return Task.FromResult(Result<Unit>.Success());
        }

        Task<Result<ImmutableArray<SeriesSeason>>> GetAllSeasons(CancellationToken _) =>
            Task.FromResult(Result<ImmutableArray<SeriesSeason>>.Success(seasons.ToImmutableArray()));
    }
}