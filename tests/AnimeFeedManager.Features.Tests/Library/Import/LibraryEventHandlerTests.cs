using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Tests.Library.Import;

public class LibraryEventHandlerTests
{
    private static LibraryEventHandler CreateHandler(ICosmosContainerFactory factory) =>
        new(factory, NullLogger<LibraryEventHandler>.Instance);

    #region Source Filter

    [Fact]
    public async Task Should_Not_Query_Container_When_Source_Is_Not_LibraryImport()
    {
        var factory = Substitute.For<ICosmosContainerFactory>();
        var handler = CreateHandler(factory);
        var evt = new OperationFailed("SomeOtherSource", "boom", DateTimeOffset.UtcNow);

        await handler.Handle(evt, TestContext.Current.CancellationToken);

        factory.DidNotReceive().GetContainer<LibraryEvent>();
    }

    [Fact]
    public async Task Should_Query_Container_When_Source_Is_LibraryImport()
    {
        var factory = Substitute.For<ICosmosContainerFactory>();
        factory.GetContainer<LibraryEvent>().Returns(CosmosInfraError.EntityNotRegistered<LibraryEvent>());
        var handler = CreateHandler(factory);
        var evt = new OperationFailed(LibrarySources.Import, "boom", DateTimeOffset.UtcNow);

        await handler.Handle(evt, TestContext.Current.CancellationToken);

        factory.Received(1).GetContainer<LibraryEvent>();
    }

    #endregion

    #region Failure Handling

    [Fact]
    public async Task Should_Not_Throw_When_Container_Lookup_Fails()
    {
        var factory = Substitute.For<ICosmosContainerFactory>();
        factory.GetContainer<LibraryEvent>().Returns(CosmosInfraError.EntityNotRegistered<LibraryEvent>());
        var handler = CreateHandler(factory);
        var evt = new OperationFailed(LibrarySources.Import, "boom", DateTimeOffset.UtcNow);

        var exception = await Record.ExceptionAsync(() => handler.Handle(evt, TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    #endregion
}
