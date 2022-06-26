namespace AnimeFeedManager.Storage.Infrastructure;

public interface ITableClientFactory<T> where T : ITableEntity
{
    TableClient GetClient();
}