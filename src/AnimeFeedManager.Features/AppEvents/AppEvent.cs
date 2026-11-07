namespace AnimeFeedManager.Features.AppEvents;

/// <summary>Something that happened to <paramref name="Subject"/>; pushed to the browser to trigger refreshes, not shown.</summary>
public sealed record AppEvent(TargetConsumer Consumer, string Type, string Subject) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "app-events";

    public string Type { get; } = string.IsNullOrWhiteSpace(Type)
        ? throw new ArgumentException("An app event needs a type", nameof(Type))
        : Type;

    public string Subject { get; } = string.IsNullOrWhiteSpace(Subject)
        ? throw new ArgumentException("An app event needs a subject", nameof(Subject))
        : Subject;

    /// <summary>The <c>{type, subject}</c> JSON the client dispatches as a DOM event.</summary>
    public string ToEnvelope() =>
        JsonSerializer.Serialize(new AppEventEnvelope(Type, Subject), AppEventsJsonContext.Default.AppEventEnvelope);

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, AppEventsJsonContext.Default.AppEvent);
    }
}

internal sealed record AppEventEnvelope(string Type, string Subject);
