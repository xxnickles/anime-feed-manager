namespace AnimeFeedManager.Functions;

public static class ContextInfo
{
    private static readonly string ActivitySourceName = typeof(ContextInfo).Assembly.GetName().Name ?? "AnimeFeedManager.Functions";
    private static readonly ActivitySource ActivitySourceInstance = new(ActivitySourceName);
    
    public static Activity? StartTracedActivity(this DomainMessage message, string operationName)
    {
        if (message.TraceInformation?.TraceParent is null)
            return null;

        // Parse the parent trace context properly
        if (!ActivityContext.TryParse(message.TraceInformation.TraceParent, message.TraceInformation.TraceState, out var parentContext))
            return null;

        // Create activity with proper parent context
        var activity = ActivitySourceInstance.StartActivity(operationName, ActivityKind.Internal, parentContext);
        
        // Add some useful tags
        activity?.SetTag("messaging.system", "azure-queue");
        activity?.SetTag("messaging.operation", "process");
        
        return activity;
    }
}