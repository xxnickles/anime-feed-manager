using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Static;

namespace AnimeFeedManager.Shared.Types;

[JsonConverter(typeof(SeriesSeasonJsonConverter))]
public sealed record SeriesSeason(Season Season, Year Year) : IComparable<SeriesSeason>
{
    public int CompareTo(SeriesSeason? other)
    {
        if (other is null) return 1;
        var yearCmp = Year.CompareTo(other.Year);
        if (yearCmp != 0) return yearCmp;
        if (Season == other.Season) return 0;
        return Season > other.Season ? 1 : -1;
    }

    public override string ToString() => $"{Year}-{Season}";

    /// <summary>
    /// Sentinel default — Winter of <see cref="Year.MinYear"/>. Used to satisfy
    /// non-null property initializers on Cosmos entities; should be replaced by
    /// real data before persistence.
    /// </summary>
    public static SeriesSeason Default { get; } = new(Season.Winter(), Year.FromNumber(Year.MinYear));

    public static bool operator >(SeriesSeason a, SeriesSeason b) => a.CompareTo(b) > 0;
    public static bool operator <(SeriesSeason a, SeriesSeason b) => a.CompareTo(b) < 0;
    public static bool operator >=(SeriesSeason a, SeriesSeason b) => a.CompareTo(b) >= 0;
    public static bool operator <=(SeriesSeason a, SeriesSeason b) => a.CompareTo(b) <= 0;
}


public static class SeriesSeasonExtensions
{
    /// <summary>
    /// Parses a (season, year) tuple into a <see cref="SeriesSeason"/>, validating
    /// both components via their existing parsers.
    /// </summary>
    public static Result<SeriesSeason> ParseAsSeriesSeason(this (string season, int year) target) =>
        target.season.ParseAsSeason().AsResult()
            .Bind(season => target.year.ParseAsYear().AsResult()
                .Map(year => new SeriesSeason(season, year)));

    /// <summary>
    /// Parses a "year-season" string (e.g., "2026-spring") into a <see cref="SeriesSeason"/>.
    /// </summary>
    public static Result<SeriesSeason> ParseAsSeriesSeason(this string value)
    {
        var parts = value.Split('-');
        if (parts.Length is not 2)
            return Validation<SeriesSeason>.Invalid(
                $"'{value}' is not a valid SeriesSeason string (expected 'year-season')").AsResult();

        if (!int.TryParse(parts[0], out var year))
            return Validation<SeriesSeason>.Invalid(
                $"'{parts[0]}' is not a valid year").AsResult();

        return (parts[1], year).ParseAsSeriesSeason();
    }
}

public class SeriesSeasonJsonConverter : JsonConverter<SeriesSeason>
{
    public override SeriesSeason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected a string value for SeriesSeason");

        var value = reader.GetString() ?? string.Empty;
        var parts = value.Split('-');
        if (parts.Length != 2)
            throw new JsonException($"'{value}' is not a valid SeriesSeason format (expected 'year-season')");

        if (!int.TryParse(parts[0], out var yearValue) || !Year.NumberIsValid(yearValue))
            throw new JsonException($"'{parts[0]}' is not a valid year");

        if (!Season.IsValid(parts[1]))
            throw new JsonException($"'{parts[1]}' is not a valid season");

        return new SeriesSeason(Season.FromString(parts[1]), Year.FromNumber(yearValue));
    }

    public override void Write(Utf8JsonWriter writer, SeriesSeason value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
