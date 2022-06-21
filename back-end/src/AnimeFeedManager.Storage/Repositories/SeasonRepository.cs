using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeFeedManager.Storage.Repositories;

public class SeasonRepository: ISeasonRepository
{
    private readonly CloudTable _tableClient;

    public SeasonRepository(ITableClientFactory<SeasonStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetCloudTable();
    }
    public Task<Either<DomainError, IEnumerable<SeasonStorage>>> GetAvailableSeasons()
    {
        var tableQuery = new TableQuery<SeasonStorage>();
        return TableUtils.TryExecuteSimpleQuery(() => _tableClient.ExecuteQuerySegmentedAsync(tableQuery, null));
    }
}