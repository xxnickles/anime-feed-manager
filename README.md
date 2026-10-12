|Project| Branch |Status|
|---|--------|---|
|Infrastructure | main | [![AMF Infrastructure](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-infrastructure.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-infrastructure.yml) |
|Functions | main | [![AMF Functions](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-functions.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-functions.yml) |
|Blazor Web | main | [![AMF Blazor Web](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-blazor.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-blazor.yml) |

Anime Feed Manager
=================

Personal anime season library and episode-feed notifier. Series metadata comes from the [AnimeSchedule](https://animeschedule.net/) API; episode feed titles are scraped from [SubsPlease](https://subsplease.org/schedule/) through a headless Chrome container. Subscribers get an email when new episodes of their series land.

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
- Docker (Aspire runs the emulators and Chrome as containers)

## Quick start

1. Add the [local configuration](#local-configuration)
2. `aspire run` from the repository root

Aspire starts Azurite (persistent data), the Azure SignalR emulator, browserless Chrome, the JS/CSS watch, Functions and the Web app; its dashboard shows logs and traces from both hosts.

## Local configuration

**Web** (user secrets):

```bash
cd src/AnimeFeedManager.Web
dotnet user-secrets set "Passwordless:ApiKey" "<api-key>"
dotnet user-secrets set "Passwordless:ApiSecret" "<api-secret>"
```

**Functions**: create `src/AnimeFeedManager.Functions/local.settings.json` (gitignored). The timer triggers read their schedules from it, so Functions won't index them without this file:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DOTNET_SYSTEM_NET_DISABLEIPV6": "1",
    "FeedNotificationSchedule": "0 0 * * * *",
    "ScrapingSchedule": "0 0 0 1 1 *",
    "FeedTitlesUpdateSchedule": "0 0 0 1 1 *",
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
