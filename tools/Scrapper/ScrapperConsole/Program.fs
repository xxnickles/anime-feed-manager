open System.IO
open Microsoft.Extensions.Configuration
open Scrappers.Common.Types
open Scrappers.Storage
open Scrappers.Scrapping
open Scrappers.Common

type TitlesStore<'a, 'b> = 'a -> Async<'b>
type ImageStore<'a, 'b> = BlobName -> 'a -> Async<'b>


type Actions =
    | Images
    | Titles
    | All

let stringToAction (actionStr: string) =
    match actionStr.ToLowerInvariant() with
    | "images" -> Images
    | "titles" -> Titles
    | "all" -> All
    | _ ->
        raise
            (System.ArgumentException
                ("No valid action was provided! Check the value 'actionToExecute' in the configuration file"))

type Command<'a> =
    { action: Actions
      description: string
      func: Async<'a> }


type Configuration =
    { storageConnection: ConnectionString
      imageContainer: ContainerName
      titlesContainer: ContainerName
      scheduleUrl: string
      imagesSourceUrl: string
      action: Actions }

let processFeedTitles url (store: TitlesStore<FeedTitles, 'b>) =
    async {
        let! browser = defaultBrowser ()
        let! r = SubsPleaseTitleScrapper browser url
        return! r |> store
    }


let processImageInfo url (store: ImageStore<ImageProcessInfo, 'b>) =
    async {
        let! browser = defaultBrowser ()
        let! r = AniDbImageScrapper browser url

        let blobName =
            sprintf "./images-%s-%i.json" r.seasonInfo.season r.seasonInfo.year
            |> BlobName

        return! r |> store blobName
    }

let getConfig (configurationRoot: IConfigurationRoot) =
    let storageConnection =
        configurationRoot.Item("storageConnection")
        |> ConnectionString

    let imageContainer =
        configurationRoot.Item("imageContainer")
        |> ContainerName

    let titlesContainer =
        configurationRoot.Item("titlesContainer")
        |> ContainerName

    let scheduleUrl = configurationRoot.Item("scheduleUrl")

    let imagesSourceUrl =
        configurationRoot.Item("imagesSourceUrl")

    let action =
        configurationRoot.Item("actionToExecute")
        |> stringToAction

    { storageConnection = storageConnection
      imageContainer = imageContainer
      titlesContainer = titlesContainer
      scheduleUrl = scheduleUrl
      imagesSourceUrl = imagesSourceUrl
      action = action }

let private getActionToExecute (commands: Command<'a> list) action  =
    let command =
        commands |> List.find (fun i -> i.action = action)

    command

let private runCommand command =
    printfn "Executing %s" command.description
    command.func |> Async.RunSynchronously |> ignore
    printfn "%s execution has been completed" command.description


let run action (commands: Command<'a> list) =
    let getCommand = getActionToExecute commands
    match action with
    | Images -> getCommand Images |> runCommand
    | Titles -> getCommand Titles |> runCommand
    | All -> commands |> List.iter runCommand

[<EntryPoint>]
let main argv =
    let configurationRoot =
        ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true)
            #if Debug
            .AddJsonFile("appsettings.development.json", true, true)
            #endif
            .Build()
            
    let config = getConfig configurationRoot

    let imgStorage =
        storeToBlob config.storageConnection config.imageContainer

    let titleStorage =
        storeToBlob config.storageConnection config.titlesContainer (BlobName "./feed-titles.json")


    let titlesProcessCmd =
        { action = Titles
          description = "Titles Scrapping"
          func = processFeedTitles config.scheduleUrl titleStorage }

    let imageProcessCmd =
        { action = Images
          description = "Image Scrapping"
          func = processImageInfo config.imagesSourceUrl imgStorage }

    let availableCommands = [ titlesProcessCmd; imageProcessCmd ]

    run config.action availableCommands

    printfn "All done!"
    
    0 // return an integer exit code
