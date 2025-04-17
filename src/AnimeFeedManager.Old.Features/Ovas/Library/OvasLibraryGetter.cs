using System.Text.Json;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Validators;
using AnimeFeedManager.Old.Features.Ovas.Library.IO;
using AnimeFeedManager.Old.Features.Ovas.Library.Types;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;
using SeriesFeedLinksContext = AnimeFeedManager.Old.Common.Domain.Types.SeriesFeedLinksContext;

namespace AnimeFeedManager.Old.Features.Ovas.Library;

public sealed class OvasLibraryGetter(IOvasSeasonalLibrary seasonalLibrary)
{
    public Task<Either<DomainError, ImmutableList<OvaLibrary>>> GetFeedForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.Season, param.Year, token))
            .MapAsync(ovas => ovas.ConvertAll(Project));
    }

    private static OvaLibrary Project(OvaStorage ovaStorage)
    {
        var id = ovaStorage.RowKey ?? string.Empty;
        var season = ovaStorage.PartitionKey ?? string.Empty;
        var title = ovaStorage.Title ?? "Not Available";
        var synopsis = ovaStorage.Synopsis ?? "Not Available";
        var imageUrl = ovaStorage.ImageUrl;
        var airDate = ovaStorage.Date;


        return new OvaLibrary(
            id,
            season,
            title,
            synopsis,
            imageUrl,
            airDate,
            !string.IsNullOrEmpty(ovaStorage.FeedInfo) 
                ? JsonSerializer.Deserialize(ovaStorage.FeedInfo,
                    SeriesFeedLinksContext.Default.SeriesFeedLinksArray) ?? []
                : []);
    }
}