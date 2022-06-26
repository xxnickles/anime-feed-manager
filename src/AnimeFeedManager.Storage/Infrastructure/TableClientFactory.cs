namespace AnimeFeedManager.Storage.Infrastructure;

public sealed class TableClientFactory<T> : ITableClientFactory<T> where T : ITableEntity
{
    private readonly TableServiceClient _serviceClient;
    private readonly Func<Type, string> _tableNameFactory;

    public TableClientFactory(
            TableServiceClient serviceClient,
            Func<Type, string> tableNameFactory)
    {
        _serviceClient = serviceClient;
        _tableNameFactory = tableNameFactory;
    }

    public TableClient GetClient()
    {
        return _serviceClient.GetTableClient(_tableNameFactory(typeof(T)));
    }
}