namespace AnimeFeedManager.Shared.Results.Static;

public static class TraceContextExtensionMembers
{
    extension(TraceContext ctx)
    {
        public bool HasEntries => ctx.LogActions.Count > 0;

        public TraceContext AddLog(Action<ILogger> logAction) =>
            ctx with { LogActions = ctx.LogActions.Add(logAction) };

        public TraceContext WithProperty(string key, object value) =>
            ctx with { Properties = ctx.Properties.SetItem(key, value) };

        public TraceContext WithProperties(IEnumerable<KeyValuePair<string, object>> props) =>
            ctx with { Properties = ctx.Properties.SetItems(props) };

        public TraceContext Merge(TraceContext other) =>
            new(ctx.LogActions.AddRange(other.LogActions), ctx.Properties.SetItems(other.Properties));

        /// <summary>Runs the log actions inside a scope holding the properties; no-op when there are no log actions.</summary>
        public void Write(ILogger logger)
        {
            if (!ctx.HasEntries)
                return;

            using (logger.BeginScope(ctx.Properties))
            {
                foreach (var action in ctx.LogActions)
                    action(logger);
            }
        }
    }
}
