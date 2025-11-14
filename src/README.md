# Anime Feed Manager back-end / Front-End

## Requirements

Please refer to [this post](https://dev.to/azure/develop-azure-functions-using-net-core-3-0-gcm) for information about the specific local setup required

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Azure SDK](https://azure.microsoft.com/en-us/downloads/)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [docker](https://www.docker.com/)

## Setup

After cloning the repository, restore the required .NET tools:

```bash
dotnet tool restore
```

This installs libman (Library Manager) which is used to manage client-side libraries for the web application.

## Required Configuration Variables (Functions)

The following are required configuration variables that need to be present when executing the host.

- **AzureWebJobsStorage**: Azure storage key. For local development "UseDevelopmentStorage=true" can be used.
- **FUNCTIONS_WORKER_RUNTIME**: use "dotnet"
- **SendGridKey**: used to send email notifications [Create account](https://signup.sendgrid.com/)
- **FromEmail** and **FromName**: email values configuration. This information is used for SendGrid
- **Sandbox**: boolean value used by SendGrid [Documentation](https://sendgrid.com/docs/for-developers/sending-email/sandbox-mode/)
- **SignalRConnectionString**: connection string for [Azure SignalR Service](https://azure.microsoft.com/en-us/products/signalr-service/)

The easiest way to configure those variables in development is creating a "local.settings.json" file in the project "AnimeFeedManager.Functions"

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "SignalRConnectionString": "[connection_string]",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "SendGridKey": "[key]",
    "FromEmail": "[email]",
    "FromName": "[name]",
    "Sandbox": true
  },
  "Host": {
    "CORS": "[Website_Url]",
    "CORSCredentials": true
  }
}
```

To make the integration with Azure Signal R possible with the website, CORS also needs to be configured as shown in the example before

## Required Configuration Variables (Blazor SSR Website)

This site uses [passwordless](https://bitwarden.com/products/passwordless/), which requires registration and configuration to generate service secrets. Also, connection string to azure storage an a reference to the functions app are also required to wire everyting together. Here is an example of a configuration file for the site:

```json
{
  "ConnectionStrings": {
    "AzureStorage": "UseDevelopmentStorage=true"
  },
  "Passwordless:ApiKey": "[api_key]",
  "Passwordless:ApiSecret": "[api_secret]",
  "Azure:SignalR:ConnectionString": "connection_string]",
  "SignalR:Endpoint": "http://localhost:7071/api/"
}
```
