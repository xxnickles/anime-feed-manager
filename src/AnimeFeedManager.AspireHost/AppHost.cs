#pragma warning disable ASPIRECOSMOSDB001

using AnimeFeedManager.Features;

var builder = DistributedApplication.CreateBuilder(args);

var databaseName = builder.Configuration["Cosmos:DatabaseName"] ?? "anime-feed";

var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator => emulator
        .WithDataExplorer()
        .WithImagePullPolicy(ImagePullPolicy.Missing)
        .WithLifetime(ContainerLifetime.Persistent));

var db = cosmos.AddCosmosDatabase(databaseName);

foreach (var (containerName, partitionKeyPath) in CosmosContainerRegistry.ContainerPartitionKeys)
    db.AddContainer(containerName, partitionKeyPath);

builder.AddProject<Projects.AnimeFeedManager_Web>("web")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
