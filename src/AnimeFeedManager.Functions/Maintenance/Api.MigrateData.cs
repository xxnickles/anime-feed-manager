using AnimeFeedManager.Features.Migration.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Maintenance;

public class MigrateData
{
    private readonly SeriesMigration _seriesMigration;
    private readonly UserMigration _userMigration;
    private readonly ILogger<MigrateData> _logger;

    public MigrateData(
        SeriesMigration seriesMigration,
        UserMigration userMigration,
        ILoggerFactory loggerFactory)
    {
        _seriesMigration = seriesMigration;
        _userMigration = userMigration;
        _logger = loggerFactory.CreateLogger<MigrateData>();
    }

    [Function("MigrateTv")]
    public async Task<HttpResponseData> RunTv(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "migration/tv")]
        HttpRequestData req)
    {
        _logger.LogInformation("Tv Data Migration Process");

        var result = await _seriesMigration.MigrateTvSeries(default);
        _logger.LogInformation("Tv Series status has been updated");
        return await result.ToResponse(req, _logger);
    }

    [Function("MigrateUser")]
    public async Task<HttpResponseData> RunUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "migration/user")]
        HttpRequestData req)
    {
        _logger.LogInformation("User Data Migration Process");

        var result = await _userMigration.MigrateUserData(default);
        _logger.LogInformation("User info has been updated");
        return await result.ToResponse(req, _logger);
    }
}