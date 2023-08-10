using System.Collections.Immutable;
using AnimeFeedManager.Application.TvAnimeLibrary.Queries;

namespace AnimeFeedManager.Application.Test.AnimeLibrary.Queries;

[Trait("Category", "AnimeLibrary Queries")]
public class GetSeasonCollectionHandlerTest
{
    [Fact]
    public async Task ShouldGetLibraryForSeason()
    {
        var handler = new GetSeasonCollectionHandler(GetMockedRepoWithResults());
        var parameters = new GetSeasonCollectionQry("fall", 2018);
        var sut = await handler.Handle(parameters, CancellationToken.None);

        Assert.True(sut.IsRight);
        sut.Match(
            value =>
            {
                Assert.Equal("fall", value.Season);
                Assert.Equal(2018, value.Year);
                Assert.Equal(10, value.Animes.Length);
            },
            _ => { });
    }

    [Fact]
    public async Task ShouldReturnValidationErrors()
    {
        var handler = new GetSeasonCollectionHandler(GetMockedRepoWithResults());
        var parameters = new GetSeasonCollectionQry("test", 1999);
        var sut = await handler.Handle(parameters, CancellationToken.None);

        Assert.True(sut.IsLeft);
        sut.Match(
            _ => { },
            error =>
            {
                Assert.IsType<ValidationErrors>(error);
                var typedError = (ValidationErrors)error;
                Assert.Equal(2, typedError.Errors.Count);
                Assert.NotEmpty(typedError.Errors["Season"]);
                Assert.NotEmpty(typedError.Errors["Year"]);
            });
    }


    private static IAnimeInfoRepository GetMockedRepoWithResults()
    {
        var mock = Substitute.For<IAnimeInfoRepository>();
        mock.GetBySeason(Arg.Any<Season>(), Arg.Any<int>()).Returns((s) =>
            Right(GetData(s.Arg<Season>().Value, (ushort)s.Arg<int>()).ToImmutableList()));

        return mock;
    }

    private static IEnumerable<AnimeInfoWithImageStorage> GetData(string season, ushort year)
    {
        for (int i = 0; i < 10; i++)
        {
            var title = $"Test {i}";

            yield return new AnimeInfoWithImageStorage
            {
                Date = DateTime.Now,
                Season = season,
                Year = year,
                PartitionKey = $"{year}-{season}",
                RowKey = i.ToString(),
                FeedTitle = title,
                ImageUrl = $"{i}.jpg",
                Title = title,
                Synopsis = "Test",
                Timestamp = DateTimeOffset.Now.AddDays(-1)
            };
        }
    }
}