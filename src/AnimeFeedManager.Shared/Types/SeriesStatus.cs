using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimeFeedManager.Shared.Types;

/// <summary>
/// Airing state of a series, aligned to Jikan's vocabulary plus an explicit
/// <see cref="Unknown"/> state for malformed or absent source data.
/// Persisted form is the upper-snake constant; display form mirrors Jikan's phrasing.
/// </summary>
[JsonConverter(typeof(SeriesStatusJsonConverter))]
public readonly record struct SeriesStatus
{
    public const string FinishedAiringValue  = "FINISHED_AIRING";
    public const string CurrentlyAiringValue = "CURRENTLY_AIRING";
    public const string NotYetAiredValue     = "NOT_YET_AIRED";
    public const string UnknownValue         = "UNKNOWN";

    private const string FinishedAiringDisplay  = "Finished Airing";
    private const string CurrentlyAiringDisplay = "Currently Airing";
    private const string NotYetAiredDisplay     = "Not yet aired";
    private const string UnknownDisplay         = "Unknown";

    private readonly string? _value;

    private SeriesStatus(string value) => _value = value;

    public override string ToString() => _value ?? UnknownValue;

    public static implicit operator string(SeriesStatus status) => status.ToString();

    public static SeriesStatus FinishedAiring()  => new(FinishedAiringValue);
    public static SeriesStatus CurrentlyAiring() => new(CurrentlyAiringValue);
    public static SeriesStatus NotYetAired()     => new(NotYetAiredValue);
    public static SeriesStatus Unknown()         => new(UnknownValue);

    /// <summary>Human-readable form (Jikan's exact phrasing) for UI display.</summary>
    public string ToDisplayString() => (_value ?? UnknownValue) switch
    {
        FinishedAiringValue  => FinishedAiringDisplay,
        CurrentlyAiringValue => CurrentlyAiringDisplay,
        NotYetAiredValue     => NotYetAiredDisplay,
        _                    => UnknownDisplay
    };

    /// <summary>Parses our canonical stored form. Unknown values fall back to <see cref="Unknown"/>.</summary>
    public static SeriesStatus FromString(string? value) => value switch
    {
        FinishedAiringValue  => FinishedAiring(),
        CurrentlyAiringValue => CurrentlyAiring(),
        NotYetAiredValue     => NotYetAired(),
        UnknownValue         => Unknown(),
        _                    => Unknown()
    };

    /// <summary>Parses Jikan's status vocabulary. Unknown values fall back to <see cref="Unknown"/>.</summary>
    public static SeriesStatus FromJikan(string? jikanStatus) => jikanStatus?.Trim() switch
    {
        FinishedAiringDisplay  => FinishedAiring(),
        CurrentlyAiringDisplay => CurrentlyAiring(),
        NotYetAiredDisplay     => NotYetAired(),
        _                      => Unknown()
    };
}

public class SeriesStatusJsonConverter : JsonConverter<SeriesStatus>
{
    public override SeriesStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();
        return SeriesStatus.FromString(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, SeriesStatus value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
