using AnimeFeedManager.Features.Domain.Validators;
using AnimeFeedManager.Features.Ovas.Library.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Library;

public sealed class OvasLibraryGetter
{
    private readonly IOvasSeasonalLibrary _seasonalLibrary;

    public OvasLibraryGetter(IOvasSeasonalLibrary seasonalLibrary)
    {
        _seasonalLibrary = seasonalLibrary;
    }

    public Task<Either<DomainError, ShortSeasonCollection>> GetForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Validate(season, year)
            .BindAsync(param => _seasonalLibrary.GetSeasonalLibrary(param.season, param.year, token))
            .MapAsync(ovas => Project(year, season, ovas));
    }

    private static ShortSeasonCollection Project(ushort year, string season,
        IEnumerable<OvaStorage> ovas)
    {
        return new ShortSeasonCollection(year, season,
            ovas.Select(a =>
                    new SimpleAnime(
                        a.RowKey ?? string.Empty,
                        a?.Title ?? "Not Available",
                        a?.Synopsis ?? "Not Available",
                        a?.ImageUrl,
                        a?.Date))
                .ToArray());
    }
}