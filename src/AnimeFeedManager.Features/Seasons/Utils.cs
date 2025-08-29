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
}