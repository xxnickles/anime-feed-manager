using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Events;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Infrastructure.Cosmos.Results;

namespace AnimeFeedManager.Features.Tests.Feeds.Classification;

// Regression check for the IHostedService -> EventSubscriber<SeasonImported> refactor: the
// season-load-fails branch (log a warning, don't throw) must behave identically post-refactor.
public class SeriesClassificationSubscriberTests
{
    private static readonly SeriesSeason Spring2026 = new(Season.Spring(), Year.FromNumber(2026));

    private static SeriesClassificationSubscriber CreateHandler(ICosmosContainerFactory factory) =>
        new(Substitute.For<IJikanClient>(), factory, NullLogger<SeriesClassificationSubscriber>.Instance);

    [Fact]
    public async Task Should_Not_Throw_When_Season_Load_Fails()
    {
        var factory = Substitute.For<ICosmosContainerFactory>();
        factory.GetContainer<Series>().Returns(CosmosInfraError.EntityNotRegistered<Series>());
        var handler = CreateHandler(factory);
        var evt = new SeasonImported(Spring2026, null, [], DateTimeOffset.UtcNow);

        var exception = await Record.ExceptionAsync(() => handler.Handle(evt, TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }
}
