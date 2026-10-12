namespace AnimeFeedManager.Functions;

public static class ContextInfo
{
    // Queue-trigger metadata key for the queue's own message id
    private const string MessageIdBindingKey = "Id";

    private static readonly ActivitySource Source = new(Telemetry.MessagingSource);

    /// <summary>Starts the <c>process {queue}</c> span: a child of the invocation span, linked to the producer's send span.</summary>
    public static Activity? StartProcessActivity(this DomainMessage message, FunctionContext functionContext)
    {
        string queue = message.MessageBox;
        ActivityLink[]? links = message.TraceInformation?.TraceParent is { } traceParent
                                && ActivityContext.TryParse(traceParent, message.TraceInformation.TraceState,
                                    out var producerContext)
            ? [new ActivityLink(producerContext)]
            : null;

        var activity = Source.StartActivity($"{Telemetry.MessagingProcessOperation} {queue}", ActivityKind.Consumer,
            parentContext: default, links: links);

        activity?.SetTag(Telemetry.MessagingSystemTag, Telemetry.MessagingSystemValue);
        activity?.SetTag(Telemetry.MessagingOperationNameTag, Telemetry.MessagingProcessOperation);
        activity?.SetTag(Telemetry.MessagingOperationTypeTag, Telemetry.MessagingProcessOperation);
        activity?.SetTag(Telemetry.MessagingDestinationNameTag, queue);
        if (functionContext.BindingContext.BindingData.TryGetValue(MessageIdBindingKey, out var messageId))
            activity?.SetTag(Telemetry.MessagingMessageIdTag, messageId?.ToString());

        return activity;
    }
}
