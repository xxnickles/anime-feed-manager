# Anime Feed Manager: source

Setup and the local quick start are in the [root README](../README.md).

## Projects

| Project | Role |
|---|---|
| `AnimeFeedManager.AspireHost` | Local orchestration: Azurite, SignalR emulator, Chrome, Functions, Web, JS/CSS watch |
| `AnimeFeedManager.ServiceDefaults` | OpenTelemetry and exporters for both hosts; HTTP resilience and health checks for Web |
| `AnimeFeedManager.Functions` | Azure Functions backend (isolated worker): scraping, imports, notifications, queue consumers |
| `AnimeFeedManager.Web` | Blazor SSR web app (htmx 4 + hyperscript) |
| `AnimeFeedManager.Web.BlazorComponents` | Shared Blazor components and email templates |
| `AnimeFeedManager.Features` | Domain features and storage, organized by feature folder |
| `AnimeFeedManager.Services.Shared` | Storage client registration and names shared by both hosts |
| `AnimeFeedManager.Shared` | Result types and telemetry constants |
| `AnimeFeedManager.SourceGenerators` | Roslyn generators ([README](AnimeFeedManager.SourceGenerators/README.md)) |

Tests live in [`../tests/`](../tests): `Features.Tests`, `Shared.Tests`, `Web.BlazorComponents.Tests`.

## Commands

Run from the repository root.

| Task | Command |
|---|---|
| Run everything | `aspire run` |
| Build | `dotnet build` |
| Test | `dotnet test -v q` (don't pass `--nologo`: under Microsoft.Testing.Platform it reports zero tests) |
| Filtered | `dotnet test -v q --project tests/<TestProject> --filter "FullyQualifiedName~<Name>"` (without `--project`, projects with no match exit non-zero) |

Frontend scripts, from `src/AnimeFeedManager.Web/`: `build:css` / `watch:css` (Tailwind), `build:js` / `watch:js`, `build` / `watch` (both). Aspire runs `watch` for you.

Client libraries (htmx, hyperscript, …) are committed under `Web/wwwroot/lib` and pinned in `Web/libman.json`. To change one, run `dotnet tool restore` from `src/` (LibMan is a local tool there), edit the pin, then `dotnet libman restore` from `src/AnimeFeedManager.Web/`. `update:libman` moves every library to its latest version; htmx's latest is still 2.x, so keep htmx pinned to 4.

## Configuration

Aspire injects storage, SignalR, Chrome and telemetry settings locally. In Azure, Bicep sets them (see [deployment/README.md](../deployment/README.md)).

### Functions

| Setting | Purpose | Required |
|---|---|---|
| `AzureWebJobsStorage` | Functions host storage | Yes |
| `Aspire:Azure:Storage:Blobs:BlobConnection:ServiceUri`, `Aspire:Azure:Storage:Queues:QueueConnection:ServiceUri`, `Aspire:Azure:Data:Tables:TablesConnection:ServiceUri` | Storage clients | Yes |
| `QueueConnection:queueServiceUri` | Queue triggers | Yes |
| `SignalRConnectionString` | Azure SignalR | Yes |
| `Chrome:RemoteEndpoint`, `Chrome:Token` | Browserless Chrome for SubsPlease scraping | Yes |
| `FeedNotificationSchedule` | Notification timer (NCrontab, 6 fields) | Yes |
| `ScrapingSchedule` | Library import timer | Yes |
| `FeedTitlesUpdateSchedule` | Feed titles timer | Yes |
| `Gmail:FromEmail`, `Gmail:FromName`, `Gmail:AppPassword` | Notification email sender | For email |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Azure Monitor exporter | In Azure |

Storage uses the `Aspire:` sections, not `ConnectionStrings:*`: a `ConnectionStrings:QueueConnection` entry makes the queue triggers parse the service URI as a connection string and fail.

### Web

| Setting | Purpose | Required |
|---|---|---|
| `ConnectionStrings:BlobConnection`, `ConnectionStrings:QueueConnection`, `ConnectionStrings:TablesConnection` | Storage clients | Yes |
| `Passwordless:ApiKey`, `Passwordless:ApiSecret` | [Passwordless.dev](https://bitwarden.com/products/passwordless/) passkeys | Yes |
| `SignalR:Endpoint` | Functions SignalR negotiation endpoint | Yes |
| `AppVersion:Version`, `AppVersion:CommitSha` | Navbar build info (default `local`) | No |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Azure Monitor exporter | In Azure |

### Telemetry (both hosts)

| Setting | Effect |
|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Enables the OTLP exporter (Aspire sets it) |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Enables the Azure Monitor exporter; both can be on at once |
| `OTEL_SERVICE_NAME` | Overrides `service.name` (falls back to `WEBSITE_SITE_NAME`, then the application name) |

Conventions for spans, log properties and keys: [docs/telemetry.md](../docs/telemetry.md).
