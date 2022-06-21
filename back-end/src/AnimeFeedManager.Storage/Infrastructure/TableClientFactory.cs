using Microsoft.Azure.Cosmos.Table;
using System;

namespace AnimeFeedManager.Storage.Infrastructure;

public sealed class TableClientFactory<T> : ITableClientFactory<T> where T : TableEntity
{
    private readonly CloudTableClient _tableClient;
    private readonly Func<Type, string> _tableNameFactory;

    public TableClientFactory(
        CloudTableClient tableClient,
        Func<Type, string> tableNameFactory)
    {
        _tableClient = tableClient;
        _tableNameFactory = tableNameFactory;
    }

    public CloudTable GetCloudTable()
    {
        return _tableClient.GetTableReference(_tableNameFactory(typeof(T)));
    }
}