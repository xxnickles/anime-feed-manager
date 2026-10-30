using System.Buffers;

namespace AnimeFeedManager.Features.AppEvents;

/// <summary>htmx <c>hx-trigger</c> values for app events re-raised on body by <c>signalr-dispatch</c>.</summary>
public static class AppEventTrigger
{
    // Each would end the filter or break the JS string literal it compiles into
    private static readonly SearchValues<char> UnsafeSubjectChars = SearchValues.Create("'\"\\]\r\n");

    /// <summary>Fires on <paramref name="type"/> events about <paramref name="subject"/>; throws when the subject can't be embedded.</summary>
    public static string On(string type, string subject) =>
        string.IsNullOrWhiteSpace(subject) || subject.AsSpan().ContainsAny(UnsafeSubjectChars)
            ? throw new ArgumentException($"'{subject}' can't be used as an app event trigger subject", nameof(subject))
            : $"{type}[detail.subject=='{subject}'] from:body";
}
