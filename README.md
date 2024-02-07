|Project| Branch |Status|
|---|--------|---|
|Infrastructure | main | [![AFM Infrastructure Deployment](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-infrastructure.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-infrastructure.yml) |
|Functions | main | [![AFM Backend Functions Deployment](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-backend-functions.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-backend-functions.yml) |
|Blazor SSR | main | [![AFM Blazor SSR Deployment](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-blazor-ssr.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/amf-blazor-ssr.yml) |
|client (Blazor SPA)| main   | [![AFM Azure Static App](https://github.com/xxnickles/anime-feed-manager/actions/workflows/azure-static-web-apps-delightful-smoke-0eded0c0f.yml/badge.svg)](https://github.com/xxnickles/anime-feed-manager/actions/workflows/azure-static-web-apps-delightful-smoke-0eded0c0f.yml)

Anime Feed Manager
=================

Simple Personal Feed Manager / Anime Season Library that uses [AniDb](https://anidb.net/) and [SubsPlease](https://subsplease.org/schedule/)*  as data sources. Simple API using [azure functions isolated](https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide), [swa cli](https://azure.github.io/static-web-apps-cli/) , Azure Storage (Tables) and [SendGrid](https://sendgrid.com). The are a couple of clients app based on Blazor [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor):

- **Blazor SPA (AnimeFeedManager.WebApp):** Webassembly Blazor
- **Blazor SSR (AnimeFeedManager.Web):** Using The SSR part of Blazor with [HTMX](https://htmx.org/) and [AlpineJS](https://alpinejs.dev/) for client interaction


_(*) This project used to use [HorribleSubs](https://horriblesubs.info/), but it closed. Then [Erai-Raws](https://spa.erai-raws.info/) was used, but their site has had multiple stability problems recently. [LiveChart](https://www.livechart.me/) has been replaced because of they moved behind to Cloudflare, which is not scrapping friendly_

## Projects

- src: Azure Functions base back-end and Blazor clients

## Dev Requirements

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [docker](https://www.docker.com/)
- [swa cli](https://azure.github.io/static-web-apps-cli/docs/cli/swa/)

## Functions local.setting.json

To run the [functions project](https://github.com/xxnickles/anime-feed-manager/tree/main/src/AnimeFeedManager.Functions), you need to create a local config with the following settings (assuming the provided docker compose is used)

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": {
      "IsEncrypted": false,
      "Values": {
        "AzureWebJobsStorage": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10001/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10002/devstoreaccount1;TableEndpoint=http://127.0.0.1:10003/devstoreaccount1;",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "Sandbox": true,
        "SendGridKey": ""

      },
      "Host": {
        "CORS": "*"
      }
    },
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Sandbox": true,
    "SendGridKey": ""

  },
  "Host": {
    "CORS": "*"
  }
}
```


