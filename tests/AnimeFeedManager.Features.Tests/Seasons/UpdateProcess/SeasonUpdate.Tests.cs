using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.Storage.Stores;
using AnimeFeedManager.Features.Seasons.UpdateProcess;

namespace AnimeFeedManager.Features.Tests.Seasons.UpdateProcess;

public class SeasonUpdateTests
{
    [Fact]
    public async Task Should_Not_Update_When_No_Update_Is_Required()
    {
        var season = new SeriesSeason(Season.Spring(), Year.FromNumber(2025));

        var seasonGetter = Substitute.For<SeasonGetter>();
        seasonGetter(season, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoUpdateRequired()));

        var latestGetter = Substitute.For<LatestSeasonGetter>();
        // Should not be called due to predicate
        var seasonUpdater = Substitute.For<SeasonUpdater>();

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(seasonGetter, season, token)
            .CreateNewSeason()
            .AddLatestSeasonData(latestGetter, token)
            .StoreUpdatedSeason(seasonUpdater, token)
            .DemoteCurrentLatest(seasonUpdater, token);

        result.AssertOnSuccess(data =>
        {
             Assert.IsType<NoUpdateRequired>(data.SeasonData);
        });

        _ = seasonUpdater.DidNotReceive()(Arg.Any<SeasonStorage>(), Arg.Any<CancellationToken>());
        _ = latestGetter.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Create_New_NonLatest_Season()
    {
        var season = new SeriesSeason(Season.Summer(), Year.FromNumber(2025), false);

        var seasonGetter = Substitute.For<SeasonGetter>();
        seasonGetter(season, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoMatch()));

        var latestGetter = Substitute.For<LatestSeasonGetter>();
        // Predicate WhenNewIsNotLatest will call this (CurrentLatestSeasonData initially NoMatch)
        latestGetter(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new CurrentLatestSeason(new SeasonStorage
            {
                PartitionKey = SeasonStorage.SeasonPartition,
                RowKey = "latest-2025",
                Latest = true,
                Season = "Spring",
                Year = 2025
            })));

        var seasonUpdater = Substitute.For<SeasonUpdater>();
        seasonUpdater(Arg.Any<SeasonStorage>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(Result<Unit>.Success()));

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(seasonGetter, season, token)
            .CreateNewSeason()
            .AddLatestSeasonData(latestGetter, token)
            .StoreUpdatedSeason(seasonUpdater, token);

        result.AssertOnSuccess(data =>
        {
            var newData = Assert.IsType<NewSeason>(data.SeasonData);
            Assert.False(newData.Season.Latest);
            Assert.Equal(season.Season.ToString(), newData.Season.Season);
            Assert.Equal(season.Year.Value, (ushort)newData.Season.Year);
        });

        _ = seasonUpdater.Received(1)(Arg.Any<SeasonStorage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Update_Existent_Season()
    {
        var existingStorage = new SeasonStorage
        {
            PartitionKey = SeasonStorage.SeasonPartition,
            RowKey = "season-2025-spring",
            Latest = false,
            Season = "Spring",
            Year = 2025
        };

        var season = new SeriesSeason(Season.Spring(), Year.FromNumber(2025));

        var seasonGetter = Substitute.For<SeasonGetter>();
        seasonGetter(season, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new ExistentSeason(existingStorage)));

        var seasonUpdater = Substitute.For<SeasonUpdater>();
        seasonUpdater(existingStorage, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success()));

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(seasonGetter, season, token)
            .CreateNewSeason()
            .StoreUpdatedSeason(seasonUpdater, token);

        result.AssertOnSuccess(data => Assert.IsType<ExistentSeason>(data.SeasonData));

        _ = seasonUpdater.Received(1)(existingStorage, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Replace_Latest_Season_And_Demote_Previous()
    {
        var incoming = new SeriesSeason(Season.Fall(), Year.FromNumber(2025), true);

        var seasonGetter = Substitute.For<SeasonGetter>();
        // Incoming is latest but not present -> NoMatch, then CreateNewSeason -> ReplaceLatestSeason
        seasonGetter(incoming, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoMatch()));

        var currentLatest = new SeasonStorage
        {
            PartitionKey = SeasonStorage.SeasonPartition,
            RowKey = "latest-2025-summer",
            Latest = true,
            Season = "Summer",
            Year = 2025
        };

        var latestGetter = Substitute.For<LatestSeasonGetter>();
        latestGetter(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new CurrentLatestSeason(currentLatest)));

        var seasonUpdater = Substitute.For<SeasonUpdater>();
        seasonUpdater(Arg.Any<SeasonStorage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success()));

        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(seasonGetter, incoming, token)
            .CreateNewSeason() // -> ReplaceLatestSeason
            .AddLatestSeasonData(latestGetter, token) // -> CurrentLatestSeason loaded
            .StoreUpdatedSeason(seasonUpdater, token) // store new latest
            .DemoteCurrentLatest(seasonUpdater, token); // demote previous latest

        result.AssertOnSuccess(data =>
        {
            Assert.IsType<NewSeason>(data.SeasonData);
            Assert.IsType<CurrentLatestSeason>(data.CurrentLatestSeasonData);
        });

        // Two updates: one for new latest, one for demotion
        _ = seasonUpdater.Received(2)(Arg.Any<SeasonStorage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Create_New_Latest_When_No_Current_Latest()
    {
        var incoming = new SeriesSeason(Season.Winter(), Year.FromNumber(2026), true);

        var seasonGetter = Substitute.For<SeasonGetter>();
        seasonGetter(incoming, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoMatch()));

        var latestGetter = Substitute.For<LatestSeasonGetter>();
        // No current latest in storage
        latestGetter(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoMatch()));

        var seasonUpdater = Substitute.For<SeasonUpdater>();
        seasonUpdater(Arg.Any<SeasonStorage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success()));
        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(seasonGetter, incoming, token)
            .CreateNewSeason()
            .AddLatestSeasonData(latestGetter, token)
            .StoreUpdatedSeason(seasonUpdater, token)
            .DemoteCurrentLatest(seasonUpdater, token);

        result.AssertOnSuccess(data =>
        {
            Assert.IsType<NewSeason>(data.SeasonData);
            Assert.IsType<NoMatch>(data.CurrentLatestSeasonData);
        });

        // Only one store call, no demotion
        _ = seasonUpdater.Received(1)(Arg.Any<SeasonStorage>(), Arg.Any<CancellationToken>());
    }
}
