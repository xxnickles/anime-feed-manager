using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Types;
using AnimeFeedManager.Features.Tv.Library;

namespace AnimeFeedManager.Web.Features.Tv;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/tv/test", async (TvLibraryGetter tvLibraryGetter) =>
            {
                var r = await tvLibraryGetter.GetForSeason(Season.Fall, Year.FromNumber(2023));

                return r.Match(
                    c => c,
                    _ => new EmptySeasonCollection()
                );
            })
           
            .WithName("GetWeatherForecast");
    }
}