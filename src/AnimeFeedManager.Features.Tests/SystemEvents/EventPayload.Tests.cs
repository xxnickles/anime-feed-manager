using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.SystemEvents;

namespace AnimeFeedManager.Features.Tests.SystemEvents;

public class EventPayloadTests
{
    [Fact]
    public void Should_Create_An_Event_Payload()
    {
        var season = new SeriesSeason(Season.Fall(), Year.FromNumber(2025));
        var payload = new TestPayload(season);
        var sut = payload.AsEventPayload();
        
        
        var deserialize = JsonSerializer.Deserialize(sut.Payload, TestPayloadContext.Default.TestPayload);
        Assert.Equal(payload, deserialize);
    }
}

public record TestPayload(SeriesSeason Season) : SerializableEventPayload<TestPayload>
{
    public override JsonTypeInfo<TestPayload> GetJsonTypeInfo() => TestPayloadContext.Default.TestPayload;
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(TestPayload))]
public partial class TestPayloadContext : JsonSerializerContext;