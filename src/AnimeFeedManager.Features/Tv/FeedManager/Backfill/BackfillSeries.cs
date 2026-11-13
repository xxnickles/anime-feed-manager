using System.Xml.Linq;
using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Completion;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Matching;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Backfill;

public sealed record BackfilledSeries(string Title, int Catalogued);

internal sealed record BackfillProcess(
    string SeriesId,
    SeriesSeason Season,
    NyaaBindingStorage Binding,
    ImmutableArray<NyaaCatalogStorage> Rows,
    CompletionOutcome Completion);

public static class BackfillSeries
{
    /// <summary>Catalogs the bound variant's releases from one nyaa search, then runs the completion rule; never notifies, and a failure is stored without a toast.</summary>
    public static Task<Result<BackfilledSeries, DomainError>> Backfill(
        BackfillNyaaSeries request,
        INyaaClient nyaaClient,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        StartProcess(request.SeriesId, clientFactory.TableStorageNyaaBindingGetter, token)
            .Search(nyaaClient, token)
            .StoreReleases(clientFactory.TableStorageNyaaCatalogUpdater, token)
            .EvaluateCompletion(clientFactory, token)
            .SendEvents(request, domainPostman, token)
            .Map(process => new BackfilledSeries(process.Binding.FeedTitle, process.Rows.Length))
            .WithOperationName(nameof(Backfill))
            .WithLogProperty(Telemetry.SeriesIdTag, request.SeriesId);

    private static Task<Result<BackfillProcess, DomainError>> StartProcess(
        string seriesId,
        NyaaBindingGetter bindingGetter,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(seriesId)
            .Bind(season => bindingGetter(seriesId, season, token)
                .Map(binding => new BackfillProcess(seriesId, season, binding, [], new FeedOpen())));

    extension(Task<Result<BackfillProcess, DomainError>> process)
    {
        private Task<Result<BackfillProcess, DomainError>> Search(INyaaClient nyaaClient, CancellationToken token) =>
            process.Bind(p => nyaaClient.Fetch(NyaaBindingQuery.For(p.Binding), token)
                .MapError(error => (DomainError)error)
                .Bind(document => Match(p, document)));

        private Task<Result<BackfillProcess, DomainError>> StoreReleases(NyaaCatalogUpdater catalogUpdater,
            CancellationToken token) =>
            process.Bind(p => catalogUpdater(p.Rows, token).Map(_ => p));

        private Task<Result<BackfillProcess, DomainError>> EvaluateCompletion(
            ITableClientFactory clientFactory,
            CancellationToken token) =>
            process.Bind(p => NyaaCompletion.CompleteWhenReached(
                    p.SeriesId,
                    p.Season,
                    clientFactory.TableStorageNyaaBindingGetter,
                    clientFactory.TableStorageNyaaCatalogGetter,
                    clientFactory.TableStorageNyaaBindingUpdater,
                    token)
                .Map(outcome => p with {Completion = outcome}));

        private Task<Result<BackfillProcess, DomainError>> SendEvents(
            BackfillNyaaSeries request,
            DomainCollectionSender domainPostman,
            CancellationToken token) =>
            process.Bind(p => domainPostman(
                        [new AppEvent(TargetConsumer.Admin(), AppEventNames.NyaaBackfilled, p.SeriesId)], token)
                    .Map(_ => p))
                .SendOnError(domainPostman,
                    [
                        new SystemEvent(TargetConsumer.Admin(), EventTarget.LocalStorage, EventType.Error,
                            new NyaaBackfillFailed(request.SeriesId).AsEventPayload())
                    ],
                    token);
    }

    private static Result<BackfillProcess, DomainError> Match(BackfillProcess process, XDocument document) =>
        NyaaFeed.Parse(document)
            .AddLogOnSuccess(feed => feed.LogErrors)
            .Map(feed => process with {Rows = NyaaMatch.CatalogRows(process.Binding, process.SeriesId, feed.Value)});
}
