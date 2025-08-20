var builder = DistributedApplication.CreateBuilder(args);
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator =>
    {
        // emulator.WithDataVolume("azurite-data");
        //emulator.WithImageTag("latest");
        emulator.WithBindMount(builder.Configuration["AzuriteDataPath"] ?? "../../../azurite-data", "/data");
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });


var blobs = storage.AddBlobs("BlobConnection");
var queues = storage.AddQueues("QueueConnection");
var tables = storage.AddTables("TablesConnection");

builder.AddNpmApp("BuildJsCss", "../AnimeFeedManager.Web", "watch");

var functions = builder.AddAzureFunctionsProject<Projects.AnimeFeedManager_Functions>("functions")
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(tables)
    .WithHostStorage(storage);

builder.AddProject<Projects.AnimeFeedManager_Web>("web")
    .WithExternalHttpEndpoints()
    .WaitFor(functions)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WithReference(queues)
    .WaitFor(queues)
    .WithReference(tables)
    .WaitFor(tables);


builder.Build().Run();