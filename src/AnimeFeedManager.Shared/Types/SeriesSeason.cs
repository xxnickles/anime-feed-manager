using System.Text.Json.Serialization;
using AnimeFeedManager.Shared.Results;

namespace AnimeFeedManager.Shared.Types;

public sealed record SeriesSeason(Season Season, Year Year, bool IsLatest = false);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SeriesSeason[]))]
public partial class SeriesSeasonContext : JsonSerializerContext;

public static class SeriesSeasonExtensions
{
    public static Result<SeriesSeason> ParseAsSeriesSeason(this (string season, int year, bool isLatest) target)
    {
        return target.season.ParseAsSeason().And(target.year.ParseAsYear())
            .Map(result => new SeriesSeason(result.Item1, result.Item2, target.isLatest));
    }
}