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

        var seasonGetter = A.Fake<SeasonGetter>();
        A.CallTo(() => seasonGetter(season, A<CancellationToken>._))
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoUpdateRequired()));

        var latestGetter = A.Fake<LatestSeasonGetter>();
        // Should not be called due to predicate
        var seasonUpdater = A.Fake<SeasonUpdater>();
        
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

        A.CallTo(() => seasonUpdater(A<SeasonStorage>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => latestGetter(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_Create_New_NonLatest_Season()
    {
        var season = new SeriesSeason(Season.Summer(), Year.FromNumber(2025), false);

        var seasonGetter = A.Fake<SeasonGetter>();
        A.CallTo(() => seasonGetter(season, A<CancellationToken>._))
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoMatch()));

        var latestGetter = A.Fake<LatestSeasonGetter>();
        // Predicate WhenNewIsNotLatest will call this (CurrentLatestSeasonData initially NoMatch)
        A.CallTo(() => latestGetter(A<CancellationToken>._))
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new CurrentLatestSeason(new SeasonStorage
            {
                PartitionKey = SeasonStorage.SeasonPartition,
                RowKey = "latest-2025",
                Latest = true,
                Season = "Spring",
                Year = 2025
            })));

        var seasonUpdater = A.Fake<SeasonUpdater>();
        A.CallTo(() => seasonUpdater(A<SeasonStorage>._, A<CancellationToken>._))
            .ReturnsLazily((SeasonStorage s, CancellationToken _) => Task.FromResult(Result<Unit>.Success()));

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

        A.CallTo(() => seasonUpdater(A<SeasonStorage>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
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

        var seasonGetter = A.Fake<SeasonGetter>();
        A.CallTo(() => seasonGetter(season, A<CancellationToken>._))
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new ExistentSeason(existingStorage)));

        var seasonUpdater = A.Fake<SeasonUpdater>();
        A.CallTo(() => seasonUpdater(existingStorage, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<Unit>.Success()));
        
        var token = CancellationToken.None;
        var result = await SeasonUpdate
            .CheckSeasonExist(seasonGetter, season, token)
            .CreateNewSeason()
            .StoreUpdatedSeason(seasonUpdater, token);

        result.AssertOnSuccess(data => Assert.IsType<ExistentSeason>(data.SeasonData));

        A.CallTo(() => seasonUpdater(existingStorage, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_Replace_Latest_Season_And_Demote_Previous()
    {
        var incoming = new SeriesSeason(Season.Fall(), Year.FromNumber(2025), true);

        var seasonGetter = A.Fake<SeasonGetter>();
        // Incoming is latest but not present -> NoMatch, then CreateNewSeason -> ReplaceLatestSeason
        A.CallTo(() => seasonGetter(incoming, A<CancellationToken>._))
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoMatch()));

        var currentLatest = new SeasonStorage
        {
            PartitionKey = SeasonStorage.SeasonPartition,
            RowKey = "latest-2025-summer",
            Latest = true,
            Season = "Summer",
            Year = 2025
        };

        var latestGetter = A.Fake<LatestSeasonGetter>();
        A.CallTo(() => latestGetter(A<CancellationToken>._))
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new CurrentLatestSeason(currentLatest)));

        var seasonUpdater = A.Fake<SeasonUpdater>();
        A.CallTo(() => seasonUpdater(A<SeasonStorage>._, A<CancellationToken>._))
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
        A.CallTo(() => seasonUpdater(A<SeasonStorage>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_Create_New_Latest_When_No_Current_Latest()
    {
        var incoming = new SeriesSeason(Season.Winter(), Year.FromNumber(2026), true);

        var seasonGetter = A.Fake<SeasonGetter>();
        A.CallTo(() => seasonGetter(incoming, A<CancellationToken>._))
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoMatch()));

        var latestGetter = A.Fake<LatestSeasonGetter>();
        // No current latest in storage
        A.CallTo(() => latestGetter(A<CancellationToken>._))
            .Returns(Task.FromResult<Result<SeasonStorageData>>(new NoMatch()));

        var seasonUpdater = A.Fake<SeasonUpdater>();
        A.CallTo(() => seasonUpdater(A<SeasonStorage>._, A<CancellationToken>._))
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
        A.CallTo(() => seasonUpdater(A<SeasonStorage>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
