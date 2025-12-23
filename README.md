|Project| Branch |Status|
|---|--------|---|
|Infrastructure | main | [![AMF Infrastructure](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-infrastructure.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-infrastructure.yml) |
|Functions | main | [![AMF Functions](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-functions.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-functions.yml) |
|Blazor Web | main | [![AMF Blazor Web](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-blazor.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-blazor.yml) |

Anime Feed Manager
=================

Simple Personal Feed Manager / Anime Season Library that uses [AniDb](https://anidb.net/) and [SubsPlease](https://subsplease.org/schedule/) as data sources. Backend powered by [Azure Functions (isolated worker)](https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide) and Azure Storage (Tables/Queues).

**Blazor SSR (AnimeFeedManager.Web):** The web application uses Blazor SSR with [HTMX](https://htmx.org/) and [AlpineJS](https://alpinejs.dev/) for client interaction.

## Dev Requirements

- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)
- [Docker](https://www.docker.com/) (for Aspire emulators)

## Quick Start

1. Clone the repository
2. Restore .NET tools: `dotnet tool restore`
3. Configure secrets (see Configuration below)
4. Run the Aspire host: `dotnet run --project src/AnimeFeedmanager.AspireHost`

The Aspire host will automatically:
- Start Azurite (Azure Storage emulator) with persistent data
- Start Azure SignalR emulator
- Build and watch frontend assets (JS/CSS)
- Launch the Functions backend
- Launch the Blazor SSR web application

## Configuration

The application requires certain configuration values to run. Aspire automatically configures Azure Storage and SignalR connections, but you need to provide:

### Web Application (User Secrets)

```bash
cd src/AnimeFeedManager.Web
dotnet user-secrets set "Passwordless:ApiKey" "[your-api-key]"
dotnet user-secrets set "Passwordless:ApiSecret" "[your-api-secret]"
```

| Setting | Description |
|---------|-------------|
| `Passwordless:ApiKey` | [Passwordless.dev](https://bitwarden.com/products/passwordless/) API key |
| `Passwordless:ApiSecret` | Passwordless.dev API secret |

### Functions (User Secrets - Optional for Email)

```bash
cd src/AnimeFeedManager.Functions
dotnet user-secrets set "Gmail:FromEmail" "[your-gmail]"
dotnet user-secrets set "Gmail:FromName" "[sender-name]"
dotnet user-secrets set "Gmail:AppPassword" "[app-password]"
```

| Setting | Description |
|---------|-------------|
| `Gmail:FromEmail` | Gmail address for sending notifications |
| `Gmail:FromName` | Display name for email sender |
| `Gmail:AppPassword` | Gmail [App Password](https://support.google.com/accounts/answer/185833) (not regular password) |

## Projects

See [src/README.md](src/README.md) for detailed project structure and configuration.

## Deployment

Deployment uses GitHub Actions with Bicep templates:
- `.github/workflows/amf-infrastructure.yml` - Infrastructure
- `.github/workflows/amf-functions.yml` - Functions
- `.github/workflows/amf-blazor.yml` - Web app
- `deployment/` - Bicep templates for Azure resources

See [deployment/README.md](deployment/README.md) for full deployment configuration guide.
