using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
    .RunAsEmulator();

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator =>
    {
        // emulator.WithDataVolume("azurite-data");
        emulator.WithImageTag("latest");
        emulator.WithBindMount(builder.Configuration["AzuriteDataPath"] ?? "../../../azurite-data", "/data");
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });

var blobs = storage.AddBlobs("BlobConnection");
var queues = storage.AddQueues("QueueConnection");
var tables = storage.AddTables("TablesConnection");

builder.AddNpmApp("BuildJsCss", "../AnimeFeedManager.Web", "watch");

var functions = builder.AddAzureFunctionsProject<Projects.AnimeFeedManager_Functions>("functions")
    .WithExternalHttpEndpoints()
    .WithHostStorage(storage)
    .WaitFor(signalR)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WithReference(queues)
    .WaitFor(queues)
    .WithReference(tables)
    .WaitFor(tables)
    .WithEnvironment("SignalRConnectionString", signalR);

builder.AddProject<Projects.AnimeFeedManager_Web>("web")
    .WaitFor(functions)
    .WaitFor(signalR)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WithReference(queues)
    .WaitFor(queues)
    .WithReference(tables)
    .WaitFor(tables)
    .WithEnvironment("SignalR__Endpoint", "http://localhost:7071/api");


builder.Build().Run();