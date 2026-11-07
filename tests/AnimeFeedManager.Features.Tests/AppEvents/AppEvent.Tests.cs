using System.Text.Json;
using AnimeFeedManager.Features.AppEvents;
using AnimeFeedManager.Features.SystemEvents;

namespace AnimeFeedManager.Features.Tests.AppEvents;

public class AppEventTests
{
    [Fact]
    public void Should_Serialize_Envelope_As_Camel_Case_Type_And_Subject_When_Sent_To_The_Client()
    {
        var appEvent = new AppEvent(TargetConsumer.Admin(), AppEventNames.NyaaBound, "series-1");

        Assert.Equal("""{"type":"nyaa:bound","subject":"series-1"}""", appEvent.ToEnvelope());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Should_Throw_When_Subject_Is_Blank(string subject)
    {
        Assert.Throws<ArgumentException>(() => new AppEvent(TargetConsumer.Admin(), AppEventNames.NyaaBound, subject));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Should_Throw_When_Type_Is_Blank(string type)
    {
        Assert.Throws<ArgumentException>(() => new AppEvent(TargetConsumer.Admin(), type, "series-1"));
    }

    [Fact]
    public void Should_Keep_Consumer_Type_And_Subject_When_Round_Tripped_Through_The_Queue()
    {
        var sent = new AppEvent(TargetConsumer.User("user-1"), AppEventNames.NyaaBackfilled, "series-1");

        var received = sent.ToBinaryData().ToObjectFromJson<AppEvent>(JsonSerializerOptions.Web);

        Assert.NotNull(received);
        Assert.Equal("user-1", received.Consumer.ToString());
        Assert.Equal(AppEventNames.NyaaBackfilled, received.Type);
        Assert.Equal("series-1", received.Subject);
        Assert.Equal(AppEvent.TargetQueue, (string)received.MessageBox);
    }
}
