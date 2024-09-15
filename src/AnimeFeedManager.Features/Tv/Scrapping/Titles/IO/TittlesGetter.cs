using AnimeFeedManager.Features.Tv.Scrapping.Titles.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;

public interface ITittlesGetter
{
    public Task<Either<DomainError, ImmutableList<string>>> GetTitles(CancellationToken token);
}

public class TittlesGetter(ITableClientFactory<TitlesStorage> tableClientFactory) : ITittlesGetter
{
    public Task<Either<DomainError, ImmutableList<string>>> GetTitles(CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<TitlesStorage>(t => t.PartitionKey == Utils.TitlesPartitionKey,
                    cancellationToken: token)))
            .BindAsync(ExtractTitles);
    }

    private static Either<DomainError, ImmutableList<string>> ExtractTitles(IImmutableList<TitlesStorage> source)
    {
        try
        {
            if (!source.Any()) return ImmutableList<string>.Empty;
            var item = source.Single();
            if (string.IsNullOrWhiteSpace(item.Titles))
            {
                return BasicError.Create("Title source contains more than one record");
            }

            return Utils.RestoreTitleCommas(item
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