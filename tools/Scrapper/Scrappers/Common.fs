module Scrappers.Common

open System.IO
open PuppeteerSharp

let defaultBrowser () =
    async {
        let downloadsFolder = Path.GetTempPath()

        let options =
            BrowserFetcherOptions(Path = downloadsFolder)


        (new BrowserFetcher(options)).DownloadAsync(BrowserFetcher.DefaultRevision)
        |> Async.AwaitTask
        |> ignore

        let lunchOptions = LaunchOptions(Headless = true)

        let! browser =
            Puppeteer.LaunchAsync(lunchOptions)
            |> Async.AwaitTask

        return browser
    }

module Types =

    [<CLIMutable>]
    type SeasonInfo = { season: string; year: int }

    [<CLIMutable>]
    type ImageInfo = { title: string; url: string }

    [<CLIMutable>]
    type ImageProcessInfo =
        { seasonInfo: SeasonInfo
          imagesInfo: ImageInfo list }


    type FeedTitles = { titles: string [] }

    type ConnectionString =
        | ConnectionString of string
        member s.Value =
            match s with
            | ConnectionString v -> v

    type ContainerName =
        | ContainerName of string
        member s.Value =
            match s with
            | ContainerName v -> v

    type BlobName =
        | BlobName of string
        member s.Value =
            match s with
            | BlobName v -> v
