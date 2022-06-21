using Microsoft.Azure.Cosmos.Table;

namespace AnimeFeedManager.Storage.Infrastructure;

public interface ITableClientFactory<T> where T : TableEntity
{
    CloudTable GetCloudTable();
}