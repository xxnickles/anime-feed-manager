#pragma warning disable ASPIRECOSMOSDB001

using AnimeFeedManager.Features;

var builder = DistributedApplication.CreateBuilder(args);

var databaseName = builder.Configuration["Cosmos:DatabaseName"] ?? "anime-feed";

var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator => emulator
        .WithDataExplorer()
        .WithDataVolume()
        .WithImagePullPolicy(ImagePullPolicy.Missing)
        .WithLifetime(ContainerLifetime.Persistent));

var db = cosmos.AddCosmosDatabase(databaseName);

foreach (var (containerName, partitionKeyPath) in CosmosContainerRegistry.ContainerPartitionKeys)
    db.AddContainer(containerName, partitionKeyPath);

var blobs = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent))
    .AddBlobs("blobs");

builder.AddProject<Projects.AnimeFeedManager_Web>("web")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(blobs)
    .WaitFor(blobs)
    // The Linux preview emulator mishandles SDK bulk execution (see CosmosOptions.AllowBulkExecution).
    // We only run against the emulator today, so disable it unconditionally.
    .WithEnvironment("Cosmos__AllowBulkExecution", "false");

builder.Build().Run();
