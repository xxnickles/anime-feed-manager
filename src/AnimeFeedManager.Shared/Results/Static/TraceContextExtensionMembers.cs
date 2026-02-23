namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Provides extension members for <see cref="TraceContext"/>,
/// enabling accumulation of log actions and properties that can be written together.
/// This supports the deferred logging pattern used by <see cref="Result{T}"/>.
/// </summary>
public static class TraceContextExtensionMembers
{
    extension(TraceContext ctx)
    {
        // ──────────────────────────────────────────────────────────────────
        // Properties
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets whether the trace context has any log actions to write.
        /// </summary>
        public bool HasEntries => ctx.LogActions.Count > 0;

        // ──────────────────────────────────────────────────────────────────
        // Log Actions
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a log action to be executed when <see cref="Write"/> is called.
        /// </summary>
        /// <param name="logAction">The logging action to add.</param>
        /// <returns>A new <see cref="TraceContext"/> with the added log action.</returns>
        public TraceContext AddLog(Action<ILogger> logAction) =>
            ctx with { LogActions = ctx.LogActions.Add(logAction) };

        // ──────────────────────────────────────────────────────────────────
        // Scope Properties
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a property that will be included in the logging scope when <see cref="Write"/> is called.
        /// </summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <returns>A new <see cref="TraceContext"/> with the added property.</returns>
        public TraceContext WithProperty(string key, object value) =>
            ctx with { Properties = ctx.Properties.SetItem(key, value) };

        /// <summary>
        /// Adds multiple properties that will be included in the logging scope when <see cref="Write"/> is called.
        /// </summary>
        /// <param name="props">The properties to add.</param>
        /// <returns>A new <see cref="TraceContext"/> with the added properties.</returns>
        public TraceContext WithProperties(IEnumerable<KeyValuePair<string, object>> props) =>
            ctx with { Properties = ctx.Properties.SetItems(props) };

        // ──────────────────────────────────────────────────────────────────
        // Composition
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Merges another <see cref="TraceContext"/> into this one.
        /// Log actions and properties from both contexts are combined.
        /// </summary>
        /// <param name="other">The trace context to merge.</param>
        /// <returns>A new <see cref="TraceContext"/> containing entries from both contexts.</returns>
        public TraceContext Merge(TraceContext other) =>
            new(ctx.LogActions.AddRange(other.LogActions), ctx.Properties.SetItems(other.Properties));

        // ──────────────────────────────────────────────────────────────────
        // Terminal
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Writes all accumulated log actions to the provided logger within a scope containing all properties.
        /// If there are no entries, this method does nothing.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
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
