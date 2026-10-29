using AnimeFeedManager.Features.Tv.FeedManager.Completion;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Management;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Episodes;

/// <param name="Bound">False when the series has no binding, so only the library holds the count.</param>
public sealed record EpisodesUpdated(int? Episodes, bool Bound, CompletionOutcome Completion);

internal sealed record EpisodesProcess(
    string SeriesId,
    SeriesSeason Season,
    int? Episodes,
    bool Bound,
    CompletionOutcome Completion);

public static class FeedEpisodes
{
    /// <summary>Copies the library count into the binding, then runs the completion rule.</summary>
    public static Task<Result<EpisodesUpdated>> UpdateFromLibrary(
        string seriesId,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        StartProcess(seriesId)
            .ReadLibrary(clientFactory.TableStorageTvSeriesGetter, token)
            .UpdateBinding(clientFactory, token)
            .EvaluateCompletion(clientFactory, domainPostman, token)
            .Map(ToUpdated)
            .WithOperationName(nameof(UpdateFromLibrary))
            .WithLogProperty(Telemetry.SeriesIdTag, seriesId);

    /// <summary>Writes the count to the library and, when bound, to the binding, then runs the completion rule.</summary>
    public static Task<Result<EpisodesUpdated>> Override(
        string seriesId,
        int episodes,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        StartProcess(seriesId)
            .Map(p => p with {Episodes = episodes})
            .UpdateLibrary(episodes, clientFactory, token)
            .UpdateBinding(clientFactory, token)
            .EvaluateCompletion(clientFactory, domainPostman, token)
            .Map(ToUpdated)
            .WithOperationName(nameof(Override))
            .WithLogProperty(Telemetry.SeriesIdTag, seriesId);

    private static Task<Result<EpisodesProcess>> StartProcess(string seriesId) =>
        Task.FromResult(IdHelpers.SeriesSeasonFromId(seriesId)
            .Map(season => new EpisodesProcess(seriesId, season, null, false, new FeedOpen())));

    extension(Task<Result<EpisodesProcess>> process)
    {
        private Task<Result<EpisodesProcess>> ReadLibrary(TvSeriesGetter seriesGetter, CancellationToken token) =>
            process.Bind(p => seriesGetter(p.SeriesId, p.Season, token)
                .Map(series => p with {Episodes = series.Episodes}));

        private Task<Result<EpisodesProcess>> UpdateLibrary(
            int episodes,
            ITableClientFactory clientFactory,
            CancellationToken token) =>
            process.Bind(p => Series.UpdateExpectedEpisodes(
                    p.SeriesId,
                    p.Season,
                    episodes,
                    clientFactory.TableStorageTvSeriesGetter,
                    clientFactory.TableStorageTvSeriesUpdater,
                    token)
                .Map(_ => p));

        private Task<Result<EpisodesProcess>> UpdateBinding(ITableClientFactory clientFactory, CancellationToken token) =>
            process.Bind(p => OptimisticConcurrency.UntilWritten(
                    attemptToken => clientFactory.TableStorageNyaaBindingGetter(p.SeriesId, p.Season, attemptToken)
                        .Bind(binding => clientFactory.TableStorageNyaaBindingUpdater(
                            WithEpisodes(binding, p.Episodes), attemptToken)),
                    token,
                    error => error is PreconditionFailedError)
                .Map(_ => p with {Bound = true})
                .BindOnErrorWhen(_ => p, error => error is NotFoundError));

        private Task<Result<EpisodesProcess>> EvaluateCompletion(
            ITableClientFactory clientFactory,
            DomainCollectionSender domainPostman,
            CancellationToken token) =>
            process.BindWhen(p => NyaaCompletion.CompleteWhenReached(
                        p.SeriesId,
                        p.Season,
                        clientFactory.TableStorageNyaaBindingGetter,
                        clientFactory.TableStorageNyaaCatalogGetter,
                        clientFactory.TableStorageNyaaBindingUpdater,
                        domainPostman,
                        token)
                    .Map(outcome => p with {Completion = outcome}),
                p => p.Bound);
    }

    private static NyaaBindingStorage WithEpisodes(NyaaBindingStorage binding, int? episodes)
    {
        binding.Episodes = episodes;
        return binding;
    }

    private static EpisodesUpdated ToUpdated(EpisodesProcess process) =>
        new(process.Episodes, process.Bound, process.Completion);
}
