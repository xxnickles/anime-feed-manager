namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

public interface ITableClientFactory
{
    Result<TableClient, StorageError> GetClient<T>() where T : ITableEntity;
}

public sealed class TableClientFactory(TableServiceClient serviceClient) : ITableClientFactory
{
    public Result<TableClient, StorageError> GetClient<T>() where T : ITableEntity
    {
        try
        {
            return serviceClient.GetTableClient(AzureTableName.GetTableName<T>());
        }
        catch (KeyNotFoundException e)
        {
            return new StorageError.TableNotConfigured(typeof(T), e);
        }
        catch (Exception e)
        {
            return new StorageError.RequestFailed("create table client", e);
        }
    }
}
