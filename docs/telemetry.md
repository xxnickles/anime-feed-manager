# Telemetry Conventions and Key Dictionary

Applies to both hosts: Web (Blazor SSR) and Functions (isolated worker). Signals flow through OpenTelemetry to Azure Monitor (App Insights) in production and to the Aspire dashboard locally. Querying it: [telemetry-queries.md](telemetry-queries.md) and the **AFM Operations** workbook.

## 1. Pipeline

| Concern | Contract |
|---|---|
| `AddServiceDefaults(sources...)` | Telemetry only. Called by both hosts. |
| `AddWebAppDefaults(sources...)` | `AddServiceDefaults` plus HTTP client resilience, service discovery, health checks (`/health`, `/alive`), ASP.NET Core instrumentation. Web only; a stacked default resilience handler would retry AnimeSchedule 429s. |
| Azure Monitor exporter | `UseAzureMonitorExporter` when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set. |
| OTLP exporter | Traces, metrics and logs when `OTEL_EXPORTER_OTLP_ENDPOINT` is set (Aspire injects it). Both exporters can be active at once. |
| Resource `service.name` | `OTEL_SERVICE_NAME` ?? `WEBSITE_SITE_NAME` ?? application name. |
| Resource `service.version` | Entry assembly informational version. |
| Resource `deployment.environment.name` | Host environment name. |
| Logs | Formatted message and scopes are exported; scope properties arrive as log attributes. |
| Traces | `Azure.*` sources, HTTP client instrumentation (suppressed when the parent span is an Azure SDK HTTP span), plus every custom source passed to the defaults call. ASP.NET Core instrumentation (Web) excludes `/health` and `/alive`. |
| Functions | Also calls `UseFunctionsWorkerDefaults`. |

**Rule:** every `ActivitySource` name in the shared `Telemetry` class must also be passed to the host's defaults call. An unregistered source has no listener: `StartActivity` returns null and its spans never exist.

## 2. Naming

| Rule | Detail |
|---|---|
| Key format | Lowercase, dotted; snake_case within a segment (`series.updated_count`). |
| Namespaces | Bare domain nouns (`series.*`, `user.*`, `feed.*`, `image.*`); no app prefix. |
| OTel namespaces | Never invent keys inside `messaging.*`, `error.*`, `url.*`, `db.*` or any other semconv namespace; reuse the semconv key. |
| Constants | Keys and span names are constants, never inline literals. |
| Exception: log templates | Template placeholders must be literals (C# templates cannot reference constants), so a key used only in a template is listed in section 7.3. |
| Value types | Primitives only; arrays comma-joined (a scope or template array stringifies as its type name). |
| Constant location | Cross-feature keys: shared `Telemetry` class. Feature keys: that feature's `*Telemetry` class. Web-only spans: internal `*Telemetry` class in the Web feature folder. Constant names end in `Tag` (keys), `Span` (span names), `Source` (source names). |

## 3. Spans

| Rule | Detail |
|---|---|
| Sources | One `ActivitySource` per capability. |
| Span name | The operation, `Feature.Operation` (e.g. `Tv.Library.Import`), as a constant in the feature's `*Telemetry` class. |
| Domain span | Started by the entry point (function, endpoint handler, component) with `using var activity = Source.StartActivity(...)`. |
| Child spans | Timing detail only (e.g. `Images.Download`); logs belong to the domain span that flushes them. |
| Logs inside the span | `FlushLogs` / `Complete` must run before the span disposes, in the same method that owns `using var activity`. |
| Errors | `MarkActivityErroredOnError()` sets status `Error` (message = domain error message) and `error.type` = `DomainError` class name (e.g. `NotFoundError`). |
| Not marked | `Warning`, `DomainValidationErrors`, `FormDataValidationError` (client input, like HTTP 4xx). `HandledError` is marked (already logged; operation still failed). |
| Mark placement | After any recovery, so recovered failures stay unmarked; inside the method that owns the span (`Activity.Current` is async-local). |
| Exceptions | Logged once via `LogError(ex)` (the exception error's log action does this). Spans never get `AddException`. |
| Errors by feature | Span-name prefix (`Tv.Library`, `Tv.Subscriptions`, ...) identifies the feature of a failure. |

Domain keys (`series.*`, `user.*`) are written as log scope properties. The only tags the existing code sets on spans are `error.type` and `messaging.*`.

## 4. Logs

| Rule | Detail |
|---|---|
| Diagnostic context | Chain `WithLogProperty` / `WithLogProperties` onto the `Result`; properties become one logging scope around the flushed log actions. Do not add extra log lines for context. |
| Operation | `WithOperationName(value)` writes `operation.name`. Log-only: reaches backends that never see span names. Value is an operation identifier (callers pass the method name, e.g. `nameof(UpsertSeries)`), not the span name. |
| Emission | `AddLog` / `AddLogOnSuccess` / `AddLogOnFailure` queue actions; `FlushLogs(logger)` writes scope + actions, appends the error's own log action on failure, then clears the context. `Complete(logger)` is the terminal form (returns void). |
| Templates | Placeholders are dotted keys: `"{series.count} TV series completed: {series.ids}"`. |
| Values | Primitives only. No objects, no arrays. |
| PII | `user.id` is an opaque id, never an email. No emails or other PII in keys, scopes, templates or span tags. |
| Failed error-event sends | `SendOnError` keeps the original error as the result and logs the failed send with it. |

## 5. Messaging

| Aspect | Producer | Consumer |
|---|---|---|
| Span name | `send {queue}` | `process {queue}` |
| Kind | Producer | Consumer |
| Source | `AnimeFeedManager.Messaging` | `AnimeFeedManager.Messaging` |
| Hosts | Web and Functions | Functions |
| Parent | Caller's current span | Functions invocation span (ambient) |
| Link | none | Producer span, parsed from the message's trace information; omitted when absent |
| `messaging.system` | `azure_storage_queue` | `azure_storage_queue` |
| `messaging.operation.name` / `.type` | `send` | `process` |
| `messaging.destination.name` | Queue name | Queue name |
| `messaging.message.id` | Queue message id from the send receipt (successful sends only) | Queue trigger `Id` binding |
| `server.address` | Queue endpoint host | not set: the consumer has no queue client |

- The producer stamps its span context (or the caller's, when the send span is unsampled) into the message's trace information; the consumer reads it to build the link.
- `messaging.message.id` is the join key across hops.
- A failed send sets span status `Error` and `error.type` = the exception's full type name (not a `DomainError` class name); the caller's `MessagesNotDelivered` logs it once.
- Every queue-triggered function starts `message.StartProcessActivity(functionContext)` first, then its domain span. Timer-triggered functions start only the domain span.

## 6. Dictionary: sources and spans

### 6.1 Sources

| Source name | Capability | Registered in |
|---|---|---|
| `AnimeFeedManager.Seasons.Update` | Season creation/update orchestration | Functions |
| `AnimeFeedManager.Tv.Library.Import` | TV library scrape/import for a season | Functions |
| `AnimeFeedManager.Tv.Library.Completion` | Marking ongoing series completed | Functions |
| `AnimeFeedManager.Tv.Library.FeedTitles` | Feed-titles scrape, match, persist | Functions |
| `AnimeFeedManager.Tv.Library.Queries` | Library catalog queries | Web |
| `AnimeFeedManager.Tv.Subscriptions.Feed` | Daily feed processing, per-user notification | Functions |
| `AnimeFeedManager.Tv.Subscriptions.Management` | Auto-subscription for interested users | Functions |
| `AnimeFeedManager.SystemEvents.Update` | System event update/dispatch | Functions |
| `AnimeFeedManager.Tv.FeedManager` | Nyaa feed bindings: bind, backfill, completion | Functions |
| `AnimeFeedManager.Messaging` | Queue send and process spans | Web, Functions |
| `AnimeFeedManager.Images` | Series image provisioning | Functions |
| `AnimeFeedManager.Web.Admin` | Admin endpoint handlers | Web |
| `AnimeFeedManager.Web.Security` | Auth/credential endpoint handlers | Web |
| `AnimeFeedManager.Web.Tv` | TV library/subscription endpoint handlers | Web |

### 6.2 Spans

| Constant | Span name | Source | Host |
|---|---|---|---|
| `SeasonsTelemetry.UpdateSpan` | `Seasons.Update` | Seasons.Update | Functions |
| `TvLibraryTelemetry.ImportSpan` | `Tv.Library.Import` | Tv.Library.Import | Functions |
| `TvLibraryTelemetry.UpdateFeedTitlesSpan` | `Tv.Library.UpdateFeedTitles` | Tv.Library.FeedTitles | Functions |
| `TvLibraryTelemetry.ScrapFeedTitlesSpan` | `Tv.Library.ScrapFeedTitles` | Tv.Library.FeedTitles | Functions |
| `TvLibraryTelemetry.CompleteOngoingSpan` | `Tv.Library.CompleteOngoing` | Tv.Library.Completion | Functions |
| `TvLibraryTelemetry.CompleteFeedSeriesSpan` | `Tv.Library.CompleteFeedSeries` | Tv.Library.Completion | Functions |
| `TvLibraryTelemetry.QuerySeasonSpan` | `Tv.Library.QuerySeason` | Tv.Library.Queries | Web |
| `TvSubscriptionsTelemetry.AutoSubscribeSpan` | `Tv.Subscriptions.AutoSubscribe` | Tv.Subscriptions.Management | Functions |
| `TvSubscriptionsTelemetry.ProcessFeedSpan` | `Tv.Subscriptions.ProcessFeed` | Tv.Subscriptions.Feed | Functions |
| `TvSubscriptionsTelemetry.NotifyUserSpan` | `Tv.Subscriptions.NotifyUser` | Tv.Subscriptions.Feed | Functions |
| `ImagesTelemetry.DownloadSpan` | `Images.Download` (child of `Tv.Library.Import`) | Images | Functions |
| `SystemEventsTelemetry.UpdateSpan` | `SystemEvents.Update` | SystemEvents.Update | Functions |
| `FeedManagerTelemetry.BindSeriesSpan` | `Tv.FeedManager.BindSeries` | Tv.FeedManager | Functions |
| `FeedManagerTelemetry.BackfillSeriesSpan` | `Tv.FeedManager.BackfillSeries` | Tv.FeedManager | Functions |
| `AdminTelemetry.UpdateTvSeasonSpan` | `Admin.UpdateTvSeason` | Web.Admin | Web |
| `AdminTelemetry.UpdateLatestTvSeasonSpan` | `Admin.UpdateLatestTvSeason` | Web.Admin | Web |
| `AdminTelemetry.UpdateFeedTitlesSpan` | `Admin.UpdateFeedTitles` | Web.Admin | Web |
| `AdminTelemetry.TriggerFeedNotificationsSpan` | `Admin.TriggerFeedNotifications` | Web.Admin | Web |
| `AdminTelemetry.SetFeaturedSeasonSpan` | `Admin.SetFeaturedSeason` | Web.Admin | Web |
| `AdminTelemetry.LibraryScrapSummarySpan` | `Admin.LibraryScrapSummary` | Web.Admin | Web |
| `AdminTelemetry.NotificationSummarySpan` | `Admin.NotificationSummary` | Web.Admin | Web |
| `AdminTelemetry.FeedUpdatesSummarySpan` | `Admin.FeedUpdatesSummary` | Web.Admin | Web |
| `TvTelemetry.ToggleInterestedSpan` | `Tv.ToggleInterested` | Web.Tv | Web |
| `TvTelemetry.ToggleSubscriptionSpan` | `Tv.ToggleSubscription` | Web.Tv | Web |
| `TvTelemetry.UpdateAlternativeTitlesSpan` | `Tv.UpdateAlternativeTitles` | Web.Tv | Web |
| `TvTelemetry.RemoveSeriesSpan` | `Tv.RemoveSeries` | Web.Tv | Web |
| `SecurityTelemetry.RegisterSpan` | `Security.Register` | Web.Security | Web |
| `SecurityTelemetry.LoginSpan` | `Security.Login` | Web.Security | Web |
| `SecurityTelemetry.AddCredentialSpan` | `Security.AddCredential` | Web.Security | Web |
| `Telemetry.MessagingSendOperation` + queue | `send {queue}` (Producer) | Messaging | Web, Functions |
| `Telemetry.MessagingProcessOperation` + queue | `process {queue}` (Consumer) | Messaging | Functions |

## 7. Dictionary: keys

### 7.1 Defined in the shared `Telemetry` class

| Key | Meaning / value format |
|---|---|
| `series.id` | Series id (table row key). |
| `series.season` | Season partition key, `2026-fall`. |
| `series.title` | Series title. |
| `user.id` | Opaque user id; never the email. |
| `user.roles` | Comma-joined role names. |
| `operation.name` | Operation identifier; log-only. |
| `table.write_retries` | Attempts `OptimisticConcurrency.UntilWritten` retried before the write landed; log-only, set only when above 0. |
| `domain_error.message` | Domain error message. |
| `domain_error.subject` | What the error concerns: expected type (type mismatch) or message type names, comma-joined (undelivered messages). |
| `validation.type_name` | Validated form/type name. |
| `validation.error_count` | Number of validation errors. |
| `validation.field_count` | Number of affected fields. |
| `validation.field` | Affected field name(s). |
| `validation.messages` | Validation message text. |

### 7.2 Defined in feature classes

| Key | Class | Meaning / value format |
|---|---|---|
| `series.count` | `TvLibraryTelemetry` | Number of series processed/affected. |
| `feed.title_count` | `TvLibraryTelemetry` | Number of feed titles. |
| `scrape.query` | `ScrappingTelemetry` | Provider request query string (AnimeSchedule, Nyaa). |
| `scrape.pages_expected` | `ScrappingTelemetry` | Pages the provider reported. |
| `scrape.pages_fetched` | `ScrappingTelemetry` | Pages actually fetched. |

### 7.3 Template-only (literals in log templates)

| Key | Meaning / value format |
|---|---|
| `domain_error.detail` | Additional message on an exception error. |
| `domain_error.actual_type` | Full type name received (type mismatch). |
| `series.updated_count` | Series updated in a run. |
| `series.new_count` | Series added in a run. |
| `series.ids` | Series ids, comma-joined. |
| `subscription.count` | Subscriptions created. |
| `notification.count` | Notifications sent. |
| `notification.user_count` | Users to be notified. |
| `schedule.next_run` | Next timer fire time. |
| `feed.title` | Feed title of a show. |
| `nyaa.group` | Fansub group of a nyaa binding. |
| `nyaa.release_title` | Group's release title of a nyaa binding. |
| `nyaa.catalogued_count` | Catalog rows written by a nyaa backfill. |
| `image.name` | Image file name. |
| `image.size` | Stored payload size in bytes. |
| `image.content_type` | Served media type. |
| `image.downloaded_count` / `.candidate_count` / `.failed_count` / `.skipped_count` | Image batch outcome counts. |
| `blob.uri` | Stored blob relative URI. |
| `blob.container` | Blob container name. |
| `table.name` | Storage table name. |
| `table.entity_type` | Entity type full name with no table mapping. |
| `scrape.entry_count` | Entries retrieved from the provider. |
| `scrape.pages_failed` | Pages skipped after failing. |

### 7.4 OpenTelemetry semantic-convention keys reused

| Key | Where | Value |
|---|---|---|
| `error.type` | Failed spans | `DomainError` class name; exception full type name on failed queue sends. |
| `messaging.system` | Send/process spans | `azure_storage_queue` (custom value; semconv registers none). |
| `messaging.operation.name`, `messaging.operation.type` | Send/process spans | `send` or `process`. |
| `messaging.destination.name` | Send/process spans; resource-creation log templates | Queue name. |
| `messaging.message.id` | Send/process spans | Queue message id. |
| `server.address` | Send spans | Queue endpoint host; App Insights fills `target` only when it is set. |
| `url.full` | Log templates | Request or page URL. |
| `service.name`, `service.version`, `deployment.environment.name` | Resource | See section 1. |

## 8. Adding telemetry

| To add | Do |
|---|---|
| Source | Add the name constant to the shared `Telemetry` class; pass it to the defaults call of every host that starts spans on it. |
| Span | Add a `Feature.Operation` constant to the feature's `*Telemetry` class (Web-only: its internal Web class); start with `using var activity = Source.StartActivity(...)`; add a row to 6.2. |
| Key | Constant in the shared class if cross-feature, otherwise in the feature's `*Telemetry` class; if it exists only as a template placeholder, add a row to 7.3. Lowercase dotted; reuse semconv inside OTel namespaces; primitive values. |
| Error handling | Call `MarkActivityErroredOnError()` after any recovery, in the method that owns the span. |
| Logs | Add context with `WithLogProperty(ies)`; call `FlushLogs` / `Complete` before the span disposes. |
| Queue consumer | Start `message.StartProcessActivity(functionContext)` before the domain span. |
| Review | No PII in keys or values; `user.id` only. |
