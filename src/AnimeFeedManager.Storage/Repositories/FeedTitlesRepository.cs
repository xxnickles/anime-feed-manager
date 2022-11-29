using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class FeedTitlesRepository : IFeedTitlesRepository
{
    private readonly TableClient _tableClient;
    private const char Comma = ',';
    private const char CommaReplacement = '#';

    public FeedTitlesRepository(ITableClientFactory<TitlesStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, ImmutableList<string>>> GetTitles() =>
        TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<TitlesStorage>(), nameof(TitlesStorage))
            .BindAsync(Mapper);


    public async Task<Either<DomainError, Unit>> MergeTitles(IEnumerable<string> titles)
    {
        var titleStorage = new TitlesStorage
        {
            Titles = string.Join(',', ReplaceTitleCommas(titles)), PartitionKey = "feed_titles", RowKey = "standard"
        };
        var result =
            await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(titleStorage), nameof(TitlesStorage));
        return result.Map(_ => unit);
    }

    // Fixes store of title with ',' which is used as separator
    // TODO: store as individual entries

    #region Coma Fixes

    private static IEnumerable<string> ReplaceTitleCommas(IEnumerable<string> source)
    {
        return source.Select(t => t.Contains(Comma) ? t.Replace(Comma, CommaReplacement) : t);
    }

    private static IEnumerable<string> RestoreTitleCommas(IEnumerable<string> source)
    {
        return source.Select(t => t.Contains(Comma) ? t.Replace(CommaReplacement, Comma) : t);
    }

    #endregion

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

            return RestoreTitleCommas(item
                    .Titles
                    .Split(',')
                    .Select(x => x.Trim()))
                .ToImmutableList();
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}