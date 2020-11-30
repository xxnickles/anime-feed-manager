|Project|Branch|Status|
|---|---|---|
|back-end|master|[![Build Status](https://dev.azure.com/afonseca0311/Anime%20Feed%20Manager/_apis/build/status/xxnickles.anime-feed-manager?branchName=master)](https://dev.azure.com/afonseca0311/Anime%20Feed%20Manager/_build/latest?definitionId=4&branchName=master)|
|client|master|[![Build Status](https://dev.azure.com/afonseca0311/Anime%20Feed%20Manager/_apis/build/status/anime-feed-manager%20client?branchName=master)](https://dev.azure.com/afonseca0311/Anime%20Feed%20Manager/_build/latest?definitionId=5&branchName=master)

Anime Feed Manager
=================

Simple Personal Feed Manager / Anime Season Library that uses [AniDb](https://anidb.net/) and [SubsPlease](https://subsplease.org/schedule/)*  as data sources. Simple API using azure functions, Azure Storage (Tables) and [SendGrid](https://sendgrid.com). The client app uses [Stencil](https://stenciljs.com/), [Ionic Components](https://ionicframework.com/docs/components) and [Akita](https://netbasal.gitbook.io/akita/)

_(*) This project used to use [HorribleSubs](https://horriblesubs.info/), but it closed. Then [Erai-Raws](https://spa.erai-raws.info/) was used, but their site has had multiple stability problems recently. [LiveChart](https://www.livechart.me/) has been replaced because of they moved behind to Cloudflare, which is not scrapping friendly_

## Projects

- back-end: Azure Functions base back-end
- client: Stencil web client
- image-scrapper: simple scrapper based on puppeteer for LiveChart anime thumbnails
