namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

public interface ITableClientFactory
{
    Result<TableClient> GetClient<T>()  where T : ITableEntity;
}

public sealed class TableClientFactory : ITableClientFactory
{
    private readonly TableServiceClient _serviceClient;
    private readonly ILogger<TableClientFactory> _logger;

    public TableClientFactory(TableServiceClient serviceClient, ILogger<TableClientFactory> logger)
    {
        _serviceClient = serviceClient;
        _logger = logger;
    }
    
    
    public Result<TableClient> GetClient<T>()  where T : ITableEntity
    {
        try
        {
            var client = _serviceClient.GetTableClient(AzureTableName.GetTableName<T>());
            return client;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Table name not found for entity type {EntityType}", typeof(T).FullName);
            return HandledErrorResult<TableClient>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when creating a Table Client");
            return HandledErrorResult<TableClient>();
        }
    }
}