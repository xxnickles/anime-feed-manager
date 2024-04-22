using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Ovas.Library.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Library;

public sealed class OvasLibraryGetter(IOvasSeasonalLibrary seasonalLibrary)
{
    public Task<Either<DomainError, ShortSeasonCollection>> GetForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.season, param.year, token))
            .MapAsync(ovas => Project(year, season, ovas));
    }

    private static ShortSeasonCollection Project(ushort year, string season,
        IEnumerable<OvaStorage> ovas)
    {
        return new ShortSeasonCollection(year, season,
            ovas.Select(a =>
                    new SimpleAnime(
                        a.RowKey ?? string.Empty,
                        a.PartitionKey ?? string.Empty,
                        a.Title ?? "Not Available",
                        a.Synopsis ?? "Not Available",
                        a.ImageUrl,
                        a?.Date))
                .ToArray());
    }
}