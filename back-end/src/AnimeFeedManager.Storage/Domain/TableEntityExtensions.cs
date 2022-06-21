using Microsoft.Azure.Cosmos.Table;

namespace AnimeFeedManager.Storage.Domain;

public static class TableEntityExtensions
{
    public static T AddEtag<T>(this T tableEntity) where T : TableEntity
    {
        tableEntity.ETag = "*";
        return tableEntity;
    }
}