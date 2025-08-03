namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

public sealed record AppTableClient(TableClient Client, ILogger Logger);

public interface ITableClientFactory
{
    Task<Result<AppTableClient>> GetClient<T>(CancellationToken cancellationToken = default)  where T : ITableEntity;
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
    
    
    public async Task<Result<AppTableClient>> GetClient<T>(CancellationToken cancellationToken = default)  where T : ITableEntity
    {
        try
        {
            var client = _serviceClient.GetTableClient(AzureTableName.GetTableName<T>());
            await client.CreateIfNotExistsAsync(cancellationToken);
            return new AppTableClient(client, _logger);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Table name not found for entity type {EntityType}", typeof(T).FullName);
            return HandledErrorResult<AppTableClient>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when creating a Table Client");
            return HandledErrorResult<AppTableClient>();
        }
    }
}