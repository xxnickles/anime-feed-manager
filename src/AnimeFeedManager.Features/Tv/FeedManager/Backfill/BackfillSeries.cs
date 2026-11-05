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
    public static Task<Result<BackfilledSeries>> Backfill(
        BackfillNyaaSeries request,
        INyaaClient nyaaClient,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        StartProcess(request.SeriesId, clientFactory.TableStorageNyaaBindingGetter, token)
            .Search(nyaaClient, token)
            .StoreReleases(clientFactory.TableStorageNyaaCatalogUpdater, token)
            .EvaluateCompletion(clientFactory, domainPostman, token)
            .SendEvents(request, domainPostman, token)
            .Map(process => new BackfilledSeries(process.Binding.FeedTitle, process.Rows.Length))
            .WithOperationName(nameof(Backfill))
            .WithLogProperty(Telemetry.SeriesIdTag, request.SeriesId);

    private static Task<Result<BackfillProcess>> StartProcess(
        string seriesId,
        NyaaBindingGetter bindingGetter,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(seriesId)
            .Bind(season => bindingGetter(seriesId, season, token)
                .Map(binding => new BackfillProcess(seriesId, season, binding, [], new FeedOpen())));

    extension(Task<Result<BackfillProcess>> process)
    {
        private Task<Result<BackfillProcess>> Search(INyaaClient nyaaClient, CancellationToken token) =>
            process.Bind(p => nyaaClient.Fetch(Query(p.Binding), token)
                .Bind(document => Match(p, document)));

        private Task<Result<BackfillProcess>> StoreReleases(NyaaCatalogUpdater catalogUpdater,
            CancellationToken token) =>
            process.Bind(p => catalogUpdater(p.Rows, token).Map(_ => p));

        private Task<Result<BackfillProcess>> EvaluateCompletion(
            ITableClientFactory clientFactory,
            DomainCollectionSender domainPostman,
            CancellationToken token) =>
            process.Bind(p => NyaaCompletion.CompleteWhenReached(
                    p.SeriesId,
                    p.Season,
                    clientFactory.TableStorageNyaaBindingGetter,
                    clientFactory.TableStorageNyaaCatalogGetter,
                    clientFactory.TableStorageNyaaBindingUpdater,
                    domainPostman,
                    token)
                .Map(outcome => p with {Completion = outcome}));

        private Task<Result<BackfillProcess>> SendEvents(
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

    // Quoted for an exact phrase match; a trusted binding filters server-side to stretch the 75-item window.
    private static NyaaQuery Query(NyaaBindingStorage binding)
    {
        var query = NyaaQuery.Create().Search($"\"[{binding.Group}] {binding.ReleaseTitle}\"");
        return binding.Trusted ? query.TrustedOnly() : query;
    }

    private static Result<BackfillProcess> Match(BackfillProcess process, XDocument document) =>
        NyaaFeed.Parse(document)
            .AddLogOnSuccess(feed => feed.LogErrors)
            .Map(feed => process with {Rows = NyaaMatch.CatalogRows(process.Binding, process.SeriesId, feed.Value)});
}
