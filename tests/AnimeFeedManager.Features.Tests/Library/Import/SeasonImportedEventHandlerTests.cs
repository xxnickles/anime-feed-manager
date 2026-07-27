using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Events;
using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Infrastructure.Cosmos.Results;

namespace AnimeFeedManager.Features.Tests.Library.Import;

public class SeasonImportedEventHandlerTests
{
    private static readonly SeriesSeason Spring2026 = new(Season.Spring(), Year.FromNumber(2026));

    private static SeasonImportedEventHandler CreateHandler(ICosmosContainerFactory factory) =>
        new(factory, NullLogger<SeasonImportedEventHandler>.Instance);

    private static SeasonImported Evt() =>
        new(Spring2026, null, [new SeriesTypeCount("tv", "TV", 1)], DateTimeOffset.UtcNow);

    [Fact]
    public async Task Should_Query_Container_When_Handling_Event()
    {
        var factory = Substitute.For<ICosmosContainerFactory>();
        factory.GetContainer<LibraryEvent>().Returns(CosmosInfraError.EntityNotRegistered<LibraryEvent>());
        var handler = CreateHandler(factory);

        await handler.Handle(Evt(), TestContext.Current.CancellationToken);

        factory.Received(1).GetContainer<LibraryEvent>();
    }

    [Fact]
    public async Task Should_Not_Throw_When_Container_Lookup_Fails()
    {
        var factory = Substitute.For<ICosmosContainerFactory>();
        factory.GetContainer<LibraryEvent>().Returns(CosmosInfraError.EntityNotRegistered<LibraryEvent>());
        var handler = CreateHandler(factory);

        var exception = await Record.ExceptionAsync(() => handler.Handle(Evt(), TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }
}
