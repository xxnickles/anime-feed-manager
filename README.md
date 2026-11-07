|Project| Branch |Status|
|---|--------|---|
|Infrastructure | main | [![AMF Infrastructure](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-infrastructure.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-infrastructure.yml) |
|Functions | main | [![AMF Functions](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-functions.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-functions.yml) |
|Blazor Web | main | [![AMF Blazor Web](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-blazor.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-blazor.yml) |

Anime Feed Manager
=================

Personal anime season library and episode-feed notifier. Series metadata comes from the [AnimeSchedule](https://animeschedule.net/) API; episode releases come from [nyaa](https://nyaa.si/) RSS, and the [SubsPlease](https://subsplease.org/schedule/) schedule binds the series it carries. Subscribers get an email when new episodes of their series land.

## What it does

| Feature | Behavior |
|---|---|
| Season library | Imports each season's TV series (titles, synopsis, cover, broadcast time, streaming links) from AnimeSchedule |
| Feed bindings | Ties a series to one nyaa release line (group + title + variant). SubsPlease series bind automatically from its schedule; any other group is bound by hand from the admin feed manager |
| Hourly nyaa run | Polls nyaa, catalogues releases of bound series (backfilling after downtime) and emails subscribers about new episodes |
| Feed settlement | Daily: marks a feed complete once its known episode count is catalogued, and retires it (completing the library series) after the v2 window |
| Releases page | Public page per series (`/tv/releases/{id}`): episodes newest first with magnet, torrent and nyaa links, revisions folded under the latest, observed weekly cadence |
| Subscriptions | Passkey sign-in; subscribe to airing series, or mark upcoming ones as interested to auto-subscribe when they start |
| Admin | Stats, live job status over SignalR, feed manager and maintenance triggers (library import, hourly nyaa run, automated binding, settlement) |

Process schedules and their Azure settings: [deployment/README.md](deployment/README.md).

| Layer | Stack |
|---|---|
| Web | Blazor SSR (no `blazor.web.js`), [htmx 4](https://four.htmx.org), [hyperscript](https://hyperscript.org/), Tailwind CSS 4 + DaisyUI 5, Passwordless.dev passkeys |
| Backend | [Azure Functions](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide) (isolated worker, .NET 10), Azure Storage tables/queues/blobs, Azure SignalR |
| Telemetry | OpenTelemetry: Aspire dashboard locally, Application Insights in Azure |
| Local orchestration | [Aspire](https://aspire.dev) |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Aspire CLI](https://aspire.dev): `curl -sSL https://aspire.dev/install.sh | bash` (PowerShell: `Invoke-RestMethod -Uri "https://aspire.dev/install.ps1" | Invoke-Expression`)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- Docker (Aspire runs the emulators as containers)

## Quick start

1. Add the [local configuration](#local-configuration)
2. `aspire run` from the repository root

Aspire starts Azurite (persistent data), the Azure SignalR emulator, the JS/CSS watch, Functions and the Web app; its dashboard shows logs and traces from both hosts.

## Local configuration

**Web** (user secrets):

```bash
cd src/AnimeFeedManager.Web
dotnet user-secrets set "Passwordless:ApiKey" "<api-key>"
dotnet user-secrets set "Passwordless:ApiSecret" "<api-secret>"
```

**Functions**: create `src/AnimeFeedManager.Functions/local.settings.json` (gitignored). It supplies the notification and import timer schedules; Aspire injects `AutomatedBindingSchedule` and `FeedSettlementSchedule`. A timer whose schedule setting is missing stops the Functions host:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DOTNET_SYSTEM_NET_DISABLEIPV6": "1",
    "FeedNotificationSchedule": "0 0 * * * *",
    "ScrapingSchedule": "0 0 0 1 1 *",
    "AzureWebJobs.ScheduledTvLibraryUpdate.Disabled": "true",
    "Gmail:FromEmail": "<gmail-address>",
    "Gmail:FromName": "Anime Feed Manager",
    "Gmail:AppPassword": "<gmail-app-password>"
  },
  "Host": {
    "CORS": "http://localhost:5020,https://localhost:7132",
    "CORSCredentials": true
  }
}
```

> **Go easy on AnimeSchedule.** It answers request bursts with a `429` that bans the caller's whole IP range for days. Keep the scheduled library update disabled locally and trigger imports from the admin UI when needed. `DOTNET_SYSTEM_NET_DISABLEIPV6` forces IPv4, so a ban on your IPv6 range doesn't block development.

Gmail settings are only needed to send notification emails; use a Gmail [app password](https://support.google.com/accounts/answer/185833), not the account password. Full settings reference: [src/README.md](src/README.md#configuration).

## Documentation

| Document | Contents |
|---|---|
| [src/README.md](src/README.md) | Project map, configuration reference, build and test commands |
| [docs/telemetry.md](docs/telemetry.md) | Telemetry conventions and key dictionary |
| [docs/telemetry-queries.md](docs/telemetry-queries.md) | App Insights KQL for ad-hoc investigation |
| [deployment/README.md](deployment/README.md) | Azure resources, GitHub secrets, workflows, schedules |
| [src/AnimeFeedManager.SourceGenerators/README.md](src/AnimeFeedManager.SourceGenerators/README.md) | Source generators |
| [src/AnimeFeedManager.Web/docs/CUSTOM-EVENTS.md](src/AnimeFeedManager.Web/docs/CUSTOM-EVENTS.md) | Custom client-side events |

## Deployment

GitHub Actions deploy Bicep infrastructure, Functions and the Web app to Azure on push to `main`; see [deployment/README.md](deployment/README.md).

## Credits

Not affiliated with any of the sites below. The app links to releases listed on nyaa; it hosts no media.

| Source | Used for |
|---|---|
| [AnimeSchedule](https://animeschedule.net/) | Season library: titles, synopses, cover art, broadcast times, streaming links |
| [nyaa](https://nyaa.si/) | Release RSS; the magnet, torrent and release-page links on the releases page |
| [SubsPlease](https://subsplease.org/) | Release schedule that drives automatic bindings, and many of the releases themselves |
| Release groups | Every group whose releases are bound and listed |
| [Vecteezy](https://www.vecteezy.com/) | 404 illustration |
| [Recraft](https://www.recraft.ai/) | Season illustrations |
| [CDNFonts](https://www.cdnfonts.com/) | Prettier Script (logo) and SF Mono (body) fonts |
| [Heroicons](https://heroicons.com/) (MIT), [Lucide](https://lucide.dev/) (ISC) | Inline SVG icons |

| Library / platform | Link |
|---|---|
| .NET, ASP.NET Core Blazor, SignalR | [dotnet.microsoft.com](https://dotnet.microsoft.com/) |
| Aspire | [aspire.dev](https://aspire.dev) |
| Azure Functions, Storage, SignalR Service, Application Insights | [azure.microsoft.com](https://azure.microsoft.com/) |
| htmx | [htmx.org](https://htmx.org/) |
| hyperscript | [hyperscript.org](https://hyperscript.org/) |
| Tailwind CSS | [tailwindcss.com](https://tailwindcss.com/) |
| daisyUI | [daisyui.com](https://daisyui.com/) |
| Chart.js | [chartjs.org](https://www.chartjs.org/) |
| Passwordless.dev (Bitwarden) | [bitwarden.com/products/passwordless](https://bitwarden.com/products/passwordless/) |
| OpenTelemetry | [opentelemetry.io](https://opentelemetry.io/) |

## AI assistance

This project is built with AI assistance. A large share of the recent code, tests and documentation was written with [Claude Code](https://claude.com/claude-code), Anthropic's agentic coding tool, working under the maintainer's direction; the maintainer reviews, verifies and commits every change.
