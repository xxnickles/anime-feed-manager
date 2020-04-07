|Project|Branch|Status|
|---|---|---|
|back-end|master|[![Build Status](https://dev.azure.com/afonseca0311/Anime%20Feed%20Manager/_apis/build/status/xxnickles.anime-feed-manager?branchName=master)](https://dev.azure.com/afonseca0311/Anime%20Feed%20Manager/_build/latest?definitionId=4&branchName=master)|
|client|master|[![Build Status](https://dev.azure.com/afonseca0311/Anime%20Feed%20Manager/_apis/build/status/anime-feed-manager%20client?branchName=master)](https://dev.azure.com/afonseca0311/Anime%20Feed%20Manager/_build/latest?definitionId=5&branchName=master)

Anime Feed Manager
=================

Simple Personal Feed Manager / Anime Season Library that uses [LiveChart](https://www.livechart.me/) and [HorribleSubs](https://horriblesubs.info/) as data sources. Simple API using azure functions, Azure Storage (Tables) and [SendGrid](https://sendgrid.com).

## Requirements

**Important!** This project is using Azure SDK Preview. Please refer to [this post](https://dev.to/azure/develop-azure-functions-using-net-core-3-0-gcm) for information about the specific local setup required

* [Dotnet Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)
* [Azure SDK](https://azure.microsoft.com/en-us/downloads/)
* [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Required Configuration Variables (Functions)

The following are required configuration variables that need to be present when executing the host.

* **AzureWebJobsStorage**: Azure storage key. For local development "UseDevelopmentStorage=true" can be used.
* **FUNCTIONS_WORKER_RUNTIME**: use "dotnet"
* **SendGridKey**: used to send email notifications [Create account](https://signup.sendgrid.com/)
* **FromEmail** and **FromName**: email values configuration. This information is used for SendGrid
* **Sandbox**: boolean value used by SendGrid [Documentation](https://sendgrid.com/docs/for-developers/sending-email/sandbox-mode/)

The easiet way to configure those variables in development is creating a "local.settings.json" file in the project "AnimeFeedManager.Functions" 

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