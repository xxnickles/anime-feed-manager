using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace AnimeFeedManager.Storage.Repositories;

public class SeasonRepository: ISeasonRepository
{
    private readonly TableClient _tableClient;

    public SeasonRepository(ITableClientFactory<SeasonStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
    }
    public Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons()
    {
        //var tableQuery = new TableQuery<SeasonStorage>();
        //return TableUtils.ExecuteQuery(() => _tableClient.ExecuteQuerySegmentedAsync(tableQuery, null));

        return TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<SeasonStorage>());
    }
}