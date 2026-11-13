using AnimeFeedManager.Features.Infrastructure.Messaging;
using Azure.Storage.Queues;

namespace AnimeFeedManager.Features.Tests.Infrastructure.Messaging;

public class AzureQueuePostmanTests
{
    #region No Target Box

    [Fact]
    public async Task Should_Fail_With_NoTargetBox_When_Single_Message_Has_No_Target()
    {
        var result = await CreatePostman().SendMessages([new UntargetedMessage()], TestContext.Current.CancellationToken);

        result.AssertOnError(error => Assert.Equal(new DeliveryError.NoTargetBox(typeof(UntargetedMessage)), error));
    }

    [Fact]
    public async Task Should_Fail_With_NoTargetBox_When_One_Message_In_A_Batch_Has_No_Target()
    {
        var result = await CreatePostman().SendMessages<DomainMessage>([new TargetedMessage(), new UntargetedMessage()], TestContext.Current.CancellationToken);

        result.AssertOnError(error => Assert.Equal(new DeliveryError.NoTargetBox(typeof(UntargetedMessage)), error));
    }

    [Fact]
    public async Task Should_Fail_With_NoTargetBox_When_Delayed_Message_Has_No_Target()
    {
        var result = await CreatePostman().SendDelayedMessage(new UntargetedMessage(), new Delay(TimeSpan.FromMinutes(1)), TestContext.Current.CancellationToken);

        result.AssertOnError(error => Assert.IsType<DeliveryError.NoTargetBox>(error));
    }

    #endregion

    #region Test Helpers

    // Never reached: every case fails before a queue call.
    private static AzureQueuePostman CreatePostman() => new(new QueueServiceClient("UseDevelopmentStorage=true"));

    private sealed record UntargetedMessage() : DomainMessage(Box.Empty())
    {
        public override BinaryData ToBinaryData() => BinaryData.FromString("{}");
    }

    private sealed record TargetedMessage() : DomainMessage(new Box("test-queue"))
    {
        public override BinaryData ToBinaryData() => BinaryData.FromString("{}");
    }

    #endregion
}
