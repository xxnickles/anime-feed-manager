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

builder.AddJavaScriptApp("BuildJsCss", "../AnimeFeedManager.Web", "watch");

// Chrome container for web scraping (browserless)
// Token for local development - production uses GitHub secret
const string chromeDevToken = "local-dev-token";

var chrome = builder.AddContainer("chrome", "ghcr.io/browserless/chromium", "latest")
    .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http")
    .WithEnvironment("TOKEN", chromeDevToken)
    .WithEnvironment("TIMEOUT", "120000")
    .WithEnvironment("CONCURRENT", "3")
    .WithEnvironment("QUEUED", "5")
    .WithEnvironment("HEALTH", "true");

var functions = builder.AddAzureFunctionsProject<Projects.AnimeFeedManager_Functions>("functions")
    .WithExternalHttpEndpoints()
    .WithHostStorage(storage)
    .WaitFor(signalR)
    .WaitFor(chrome)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WithReference(queues)
    .WaitFor(queues)
    .WithReference(tables)
    .WaitFor(tables)
    .WithEnvironment("SignalRConnectionString", signalR)
    .WithEnvironment("Chrome__RemoteEndpoint", chrome.GetEndpoint("http"))
    .WithEnvironment("Chrome__Token", chromeDevToken);

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