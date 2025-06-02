using AnimeFeedManager.Features.Seasons.UpdateProcess;

namespace AnimeFeedManager.Features.Seasons;

internal static class Utils
{
    /// <summary>
    /// Specific binder that will skip the process when Season Already exist in storage 
    /// </summary>
    /// <param name="resultTask"></param>
    /// <param name="binder"></param>
    /// <param name="discriminator"></param>
    /// <returns></returns>
    internal static Task<Result<SeasonUpdateData>> BindSeasonData(this Task<Result<SeasonUpdateData>> resultTask,
        Func<SeasonUpdateData, Task<Result<SeasonUpdateData>>> binder, Func<SeasonUpdateData, bool> discriminator)
    {
        return resultTask.Bind(v =>
            discriminator(v) ? binder(v) : Task.FromResult(Result<SeasonUpdateData>.Success(v)));
    }

    private static bool IsSameSeason(SeasonStorage a, SeasonStorage b) =>
        a.Season == b.Season && a.Year == b.Year;

    internal static bool IsSameSeasonData(SeasonStorageData a, SeasonStorageData b) => (a, b) switch
    {
        (LatestSeason seasonA, LatestSeason seasonB) => IsSameSeason(seasonA.Season, seasonB.Season),
        (ExistentSeason seasonA, LatestSeason seasonB) => IsSameSeason(seasonA.Season, seasonB.Season),
        (LatestSeason seasonA, ExistentSeason seasonB) => IsSameSeason(seasonA.Season, seasonB.Season),
        _ => false,
    };
}