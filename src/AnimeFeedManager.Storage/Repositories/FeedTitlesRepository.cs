using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class FeedTitlesRepository : IFeedTitlesRepository
{
    private readonly TableClient _tableClient;

    public FeedTitlesRepository(ITableClientFactory<TitlesStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, ImmutableList<string>>> GetTitles() =>
        TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<TitlesStorage>()).BindAsync(Mapper);


    public async Task<Either<DomainError, Unit>> MergeTitles(IEnumerable<string> titles)
    {
        var titleStorage = new TitlesStorage { Titles = string.Join(',', titles), PartitionKey = "feed_titles", RowKey = "standard" };
        var result = await TableUtils.TryExecute(() => Task.FromResult(_tableClient.UpsertEntityAsync(titleStorage)));

        return result.Map(_ => unit);
    }

    private static Either<DomainError, ImmutableList<string>> Mapper(IEnumerable<TitlesStorage> source)
    {
        try
        {
            if (!source.Any()) return ImmutableList<string>.Empty;
            var item = source.Single();
            if (string.IsNullOrWhiteSpace(item.Titles))
            {
                return BasicError.Create(nameof(FeedTitlesRepository), "Title source contains more than one record");
            }

            return item
                .Titles
                .Split(',')
                .Select(x => x.Trim())
                .ToImmutableList();

        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}