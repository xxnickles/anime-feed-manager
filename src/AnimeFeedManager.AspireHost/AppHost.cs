using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
    .RunAsEmulator(emulator => emulator.WithImageTag("1.7.0"));

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator.WithImageTag("3.37.0")
            .WithImagePullPolicy(ImagePullPolicy.Missing)
            .WithBindMount(builder.Configuration["AzuriteDataPath"] ?? "../../../azurite-data", "/data")
            .WithLifetime(ContainerLifetime.Persistent)
    );

var blobs = storage.AddBlobs("BlobConnection");
var queues = storage.AddQueues("QueueConnection");
var tables = storage.AddTables("TablesConnection");

builder.AddJavaScriptApp("BuildJsCss", "../AnimeFeedManager.Web", "watch");

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
    .WithEnvironment("SignalRConnectionString", signalR)
    // Here, not local.settings.json (gitignored): an unresolved timer setting stops the host
    .WithEnvironment("AutomatedBindingSchedule", "0 0 5 * * *");

var commitSha = GetLocalCommitSha();

var web = builder.AddProject<Projects.AnimeFeedManager_Web>("web")
    .WaitFor(functions)
    .WithReference(signalR)
    .WaitFor(signalR)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WithReference(queues)
    .WaitFor(queues)
    .WithReference(tables)
    .WaitFor(tables)
    .WithEnvironment("AppVersion__CommitSha", commitSha);

// Endpoint reference only; a WaitFor here would cycle with web's WaitFor(functions)
functions.WithEnvironment("WebApp__BaseUrl", web.GetEndpoint("https"));


builder.Build().Run();
return;

static string GetLocalCommitSha()
{
    try
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --short HEAD",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        var sha = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        return process.ExitCode == 0 ? sha : string.Empty;
    }
    catch
    {
        return string.Empty;
    }
}