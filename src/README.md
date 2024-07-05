Anime Feed Manager back-end
=================

## Requirements

Please refer to [this post](https://dev.to/azure/develop-azure-functions-using-net-core-3-0-gcm) for information about the specific local setup required

* [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [Azure SDK](https://azure.microsoft.com/en-us/downloads/)
* [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
* [docker](https://www.docker.com/)

## Required Configuration Variables (Functions)

The following are required configuration variables that need to be present when executing the host.

* **AzureWebJobsStorage**: Azure storage key. For local development "UseDevelopmentStorage=true" can be used.
* **FUNCTIONS_WORKER_RUNTIME**: use "dotnet"
* **SendGridKey**: used to send email notifications [Create account](https://signup.sendgrid.com/)
* **FromEmail** and **FromName**: email values configuration. This information is used for SendGrid
* **Sandbox**: boolean value used by SendGrid [Documentation](https://sendgrid.com/docs/for-developers/sending-email/sandbox-mode/)

The easiest way to configure those variables in development is creating a "local.settings.json" file in the project "AnimeFeedManager.Functions" 

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "SendGridKey": "[key]",
    "FromEmail": "[email]",
    "FromName": "[name]",
    "Sandbox": true
  }
}
```