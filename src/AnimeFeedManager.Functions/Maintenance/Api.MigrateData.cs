using AnimeFeedManager.Features.Migration.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Maintenance;

public class MigrateData(
    SeriesMigration seriesMigration,
    UserMigration userMigration,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<MigrateData> _logger = loggerFactory.CreateLogger<MigrateData>();

    [Function("MigrateTv")]
    public async Task<HttpResponseData> RunTv(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "migration/tv")]
        HttpRequestData req, CancellationToken token)
    {
        _logger.LogInformation("Tv Data Migration Process");

        var result = await seriesMigration.MigrateTvSeries(token);
        _logger.LogInformation("Tv Series status has been updated");
        return await result.ToResponse(req, _logger);
    }

    [Function("MigrateUser")]
    public async Task<HttpResponseData> RunUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "migration/user")]
        HttpRequestData req)
    {
        _logger.LogInformation("User Data Migration Process");

        var result = await userMigration.MigrateUserData(default);
        _logger.LogInformation("User info has been updated");
        return await result.ToResponse(req, _logger);
    }
}