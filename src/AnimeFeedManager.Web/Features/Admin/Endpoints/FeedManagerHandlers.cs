using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Completion;
using AnimeFeedManager.Features.Tv.FeedManager.Episodes;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Queries;
using AnimeFeedManager.Features.Tv.FeedManager.Search;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Web.BlazorComponents.Toast;
using AnimeFeedManager.Web.Features.Admin.FeedManager;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class FeedManagerHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebAdminSource);

    internal static async Task<RazorComponentResult> Bind(
        [FromForm] BindReleaseViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        [FromServices] ILogger<ReleaseCandidateRow> logger,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(AdminTelemetry.BindNyaaSeriesSpan);
        return await Validate(viewModel)
            .Bind(vm => domainPostman.SendMessages(
                [new BindNyaaSeries(vm.SeriesId, vm.Group, vm.ReleaseTitle, vm.Variant ?? string.Empty, vm.Trusted)],
                token))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                _ =>
                [
                    Notifications.CreateNotificationToast("Feed binding",
                        Notifications.TextBody(
                            $"{viewModel.SeriesTitle} will be bound to [{viewModel.Group}] and indexed in the background."),
                        ToastType.Info)
                ],
                error => [Notifications.CreateErrorNotification("Feed binding", error)]);
    }

    internal static async Task<RazorComponentResult> Backfill(
        [FromForm] FeedSeriesViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        [FromServices] ILogger<CurrentFeedCard> logger,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(AdminTelemetry.BackfillNyaaSeriesSpan);
        return await Validate(viewModel)
            .Bind(vm => domainPostman.SendMessages([new BackfillNyaaSeries(vm.SeriesId)], token))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                _ =>
                [
                    Notifications.CreateNotificationToast("Backfill",
                        Notifications.TextBody($"{viewModel.SeriesTitle} will be backfilled in the background."),
                        ToastType.Info)
                ],
                error => [Notifications.CreateErrorNotification("Backfill", error)]);
    }

    internal static async Task<RazorComponentResult> ClearCatalog(
        [FromForm] FeedSeriesViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<CurrentFeedCard> logger,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(AdminTelemetry.ClearNyaaCatalogSpan);
        return await Validate(viewModel)
            .Bind(vm => clientFactory.TableStorageNyaaCatalogWiper(vm.SeriesId, token))
            .Bind(_ => Reload(viewModel.SeriesId, clientFactory, token))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                series =>
                [
                    BindingCards.AsOobFragment(series),
                    Notifications.CreateNotificationToast("Catalog cleared",
                        Notifications.TextBody($"{viewModel.SeriesTitle} has no catalogued releases. Backfill to index the bound release again."))
                ],
                error => [Notifications.CreateErrorNotification("Clear catalog", error)]);
    }

    internal static async Task<RazorComponentResult> UpdateEpisodesFromLibrary(
        [FromForm] FeedSeriesViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<EpisodesCard> logger,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(AdminTelemetry.UpdateFeedEpisodesSpan);
        return await Validate(viewModel)
            .Bind(vm => FeedEpisodes.UpdateFromLibrary(vm.SeriesId, clientFactory, token))
            .Bind(updated => Reload(viewModel.SeriesId, clientFactory, token).Map(series => (Updated: updated, Series: series)))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                result =>
                [
                    BindingCards.AsOobFragment(result.Series),
                    Notifications.CreateNotificationToast("Feed episodes",
                        Notifications.TextBody(FromLibraryMessage(viewModel.SeriesTitle, result.Updated)))
                ],
                error => [Notifications.CreateErrorNotification("Feed episodes", error)]);
    }

    internal static async Task<RazorComponentResult> OverrideEpisodes(
        [FromForm] EpisodesOverrideViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<EpisodesCard> logger,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(AdminTelemetry.OverrideEpisodesSpan);
        return await Validate(viewModel)
            .Bind(vm => FeedEpisodes.Override(vm.SeriesId, vm.Episodes, clientFactory, token))
            .Bind(updated => Reload(viewModel.SeriesId, clientFactory, token).Map(series => (Updated: updated, Series: series)))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                result =>
                [
                    BindingCards.AsOobFragment(result.Series),
                    Notifications.CreateNotificationToast("Episodes override",
                        Notifications.TextBody(OverrideMessage(viewModel.SeriesTitle, result.Updated)))
                ],
                error => [Notifications.CreateErrorNotification("Episodes override", error)]);
    }

    internal static async Task<RazorComponentResult> Cards(
        string seriesId,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<BindingCards> logger,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(AdminTelemetry.RefreshBindingCardsSpan);
        return await Reload(seriesId, clientFactory, token)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                series => [BindingCards.AsOobFragment(series)],
                error => [Notifications.CreateErrorNotification("Feed cards", error)]);
    }

    internal static async Task<RazorComponentResult> Candidates(
        string seriesId,
        [FromQuery] string q,
        [FromQuery] bool trusted,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] INyaaClient nyaaClient,
        [FromServices] ILogger<ReleaseCandidates> logger,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(AdminTelemetry.RefreshReleaseCandidatesSpan);
        return await Reload(seriesId, clientFactory, token)
            .Bind(series => ReleaseSearch.Search(q, trusted, series.Binding, nyaaClient, token)
                .Map(results => (Series: series, Results: results)))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                result => [ReleaseCandidates.AsOobFragment(result.Series, q, trusted, result.Results)],
                error => [Notifications.CreateErrorNotification("Release search", error)]);
    }

    private static Task<Result<FeedManagerSeries>> Reload(
        string seriesId,
        ITableClientFactory clientFactory,
        CancellationToken token) =>
        FeedManagerSeriesQuery.GetFeedManagerSeries(
            seriesId,
            clientFactory.TableStorageTvSeriesGetter,
            clientFactory.TableStorageNyaaBindingGetter,
            clientFactory.TableStorageNyaaCatalogGetter,
            DateTimeOffset.UtcNow,
            token);

    private static string FromLibraryMessage(string title, EpisodesUpdated updated) =>
        $"{title}: feed count set to {updated.Episodes?.ToString() ?? "unknown"}.{CompletionNote(updated)}";

    private static string OverrideMessage(string title, EpisodesUpdated updated) =>
        $"{title}: {updated.Episodes} episodes saved to {(updated.Bound ? "the library and the feed" : "the library")}.{CompletionNote(updated)}";

    private static string CompletionNote(EpisodesUpdated updated) =>
        updated.Completion is FeedCompleted ? " The feed reached its count; the series completes when the feed retires." : string.Empty;
}
