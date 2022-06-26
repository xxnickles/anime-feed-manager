namespace AnimeFeedManager.Storage.Domain;

public static class TableEntityExtensions
{
    public static T AddEtag<T>(this T tableEntity) where T : ITableEntity
    {
        tableEntity.ETag = new ETag("*");
        return tableEntity;
    }
}