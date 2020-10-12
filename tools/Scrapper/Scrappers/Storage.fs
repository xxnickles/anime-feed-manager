module Scrappers.Storage

open System.IO
open System.Text
open Azure.Storage.Blobs
open Azure.Storage.Blobs.Models
open Newtonsoft.Json
open Scrappers.Common.Types

let serialize = JsonConvert.SerializeObject

let storeToBlob<'T> (connectionString: ConnectionString) (containerName: ContainerName) (blobName: BlobName) (content: 'T) =
    async {

        let container =
            new BlobContainerClient(connectionString.Value, containerName.Value)

        let! _ =
            container.CreateIfNotExistsAsync()
            |> Async.AwaitTask

        let jsonString = serialize content

        let client = container.GetBlobClient(blobName.Value)

        let blobHeaders =
            new BlobHttpHeaders(ContentType = "application/json")

        use memoryStream =
            new MemoryStream(Encoding.UTF8.GetBytes(jsonString))

        let! response =
            client.UploadAsync(memoryStream, blobHeaders)
            |> Async.AwaitTask

        return response.Value
    }


let storeToDisk<'T> path filename (content: 'T) =
      async {
        let jsonString =  serialize content
        use writer = new StreamWriter(sprintf "%s/%s.json" path filename)
        return! writer.WriteAsync(jsonString) |> Async.AwaitTask        
      }
      
    