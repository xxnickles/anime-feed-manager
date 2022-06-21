using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using Azure;
using Azure.Data.Tables;
using LanguageExt;
using System.Collections.Immutable;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage.Repositories;

public class AnimeInfoRepository : IAnimeInfoRepository
{
    private readonly TableClient _tableClient;

    public AnimeInfoRepository(ITableClientFactory<AnimeInfoStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
    }

    public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetBySeason(Season season, int year)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, (ushort)year);
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<AnimeInfoWithImageStorage>(a => a.PartitionKey == partitionKey));

    }

    public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetIncomplete()
    {
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<AnimeInfoWithImageStorage>(a =>
                a.Completed == false && a.FeedTitle != string.Empty));
    }

    public Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> GetAll() =>
        TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<AnimeInfoStorage>());


    public async Task<Either<DomainError, Unit>> Merge(AnimeInfoStorage animeInfo)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpdateEntityAsync(animeInfo, ETag.All));
        return result.Map(_ => unit);
    }

    public async Task<Either<DomainError, Unit>> AddImageUrl(ImageStorage image)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpdateEntityAsync(image, ETag.All));
        return result.Map(_ => unit);
    }
}