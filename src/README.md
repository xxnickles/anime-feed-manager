# Anime Feed Manager

## Requirements

- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)
- [Docker](https://www.docker.com/)

## Setup

After cloning the repository, restore the required .NET tools:

```bash
dotnet tool restore
```

This installs libman (Library Manager) which is used to manage client-side libraries for the web application.

## Project Structure

| Project | Description |
|---------|-------------|
| **AnimeFeedManager.AspireHost** | .NET Aspire orchestration for local development |
| **AnimeFeedManager.Features** | Business logic and domain features |
| **AnimeFeedManager.Features.Tests** | xUnit tests for Features |
| **AnimeFeedManager.Functions** | Azure Functions backend (isolated worker) |
| **AnimeFeedManager.ServiceDefaults** | .NET Aspire service defaults |
| **AnimeFeedManager.Services.Shared** | Shared service registrations |
| **AnimeFeedManager.Shared** | Shared types and Result pattern |
| **AnimeFeedManager.SourceGenerators** | Roslyn source generators |
| **AnimeFeedManager.Web** | Blazor SSR web application |
| **AnimeFeedManager.Web.BlazorComponents** | Shared Blazor components and email templates |

## Documentation

| Document | Description |
|----------|-------------|
| [Custom JavaScript Events](AnimeFeedManager.Web/docs/CUSTOM-EVENTS.md) | Reference for all custom JS events used in the Web project (HTMX triggers, Alpine.js events, SignalR events) |

## Running Locally

The preferred way to run locally is using the Aspire host:

```bash
dotnet run --project AnimeFeedmanager.AspireHost
```

This starts all services with proper configuration, including:
- Azurite (Azure Storage emulator)
- Azure SignalR emulator
- Browserless Chrome container (for web scraping)
- Frontend asset watch (Tailwind CSS/JS)
- Functions backend
- Web application

### Running Individual Projects

To run projects individually (for debugging), you'll need to configure the connection strings manually.

#### Functions

Create `local.settings.json` in `AnimeFeedManager.Functions`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10001/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10002/devstoreaccount1;TableEndpoint=http://127.0.0.1:10003/devstoreaccount1;",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Chrome__RemoteEndpoint": "http://localhost:3000",
    "Chrome__Token": "local-dev-token"
  },
  "Host": {
    "CORS": "*"
  }
}
```

**Note:** Chrome settings require running the browserless container separately:
```bash
docker run -p 3000:3000 -e TOKEN=local-dev-token ghcr.io/browserless/chromium:latest
```

For email notifications, add Gmail configuration via user secrets:

```bash
dotnet user-secrets set "Gmail:FromEmail" "[your-gmail]"
dotnet user-secrets set "Gmail:FromName" "[sender-name]"
dotnet user-secrets set "Gmail:AppPassword" "[app-password]"
```

#### Web Application

Configure user secrets for the Web project:

```bash
cd AnimeFeedManager.Web
dotnet user-secrets set "ConnectionStrings:TablesConnection" "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10003/devstoreaccount1;"
dotnet user-secrets set "Passwordless:ApiKey" "[api-key]"
dotnet user-secrets set "Passwordless:ApiSecret" "[api-secret]"
dotnet user-secrets set "SignalR:Endpoint" "http://localhost:7071/api/"
```

## Configuration Reference

### Functions

| Setting | Description | Required |
|---------|-------------|----------|
| `AzureWebJobsStorage` | Azure Storage connection string (local) or account name (Azure) | Yes |
| `FUNCTIONS_WORKER_RUNTIME` | Must be `dotnet-isolated` | Yes |
| `SignalRConnectionString` | Azure SignalR connection (provided by Aspire) | Yes |
| `Chrome:RemoteEndpoint` | Browserless Chrome WebSocket endpoint (provided by Aspire) | Yes |
| `Chrome:Token` | Authentication token for Chrome container | Yes |
| `Gmail:FromEmail` | Gmail address for notifications | No |
| `Gmail:FromName` | Email sender display name | No |
| `Gmail:AppPassword` | Gmail App Password | No |
| `FeedNotificationSchedule` | Cron schedule for feed notifications (default: `0 0 * * * *` hourly) | No |
| `ScrapingSchedule` | Cron schedule for library scraping (default: `0 0 4 * * 6` weekly) | No |
| `FeedTitlesUpdateSchedule` | Cron schedule for feed titles update (default: `0 0 0 1 1 *` disabled) | No |

### Web Application

| Setting | Description | Required |
|---------|-------------|----------|
| `ConnectionStrings:TablesConnection` | Azure Table Storage connection | Yes |
| `ConnectionStrings:BlobConnection` | Azure Blob Storage connection | Yes |
| `ConnectionStrings:QueueConnection` | Azure Queue Storage connection | Yes |
| `Passwordless:ApiKey` | Passwordless.dev API key | Yes |
| `Passwordless:ApiSecret` | Passwordless.dev API secret | Yes |
| `SignalR:Endpoint` | Functions SignalR endpoint | Yes |
| `AppVersion:Version` | Build version displayed in navbar (default: `local`) | No |
| `AppVersion:CommitSha` | Short commit SHA for navbar link (injected by Aspire locally) | No |

## Build Commands

```bash
# Build all projects
dotnet build

# Run tests
dotnet test AnimeFeedManager.Features.Tests

# Run specific test
dotnet test AnimeFeedManager.Features.Tests --filter "FullyQualifiedName~TestName"

# Publish Functions
dotnet publish AnimeFeedManager.Functions -c Release -o ./output
```
