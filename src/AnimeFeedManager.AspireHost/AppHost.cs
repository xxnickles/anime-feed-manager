#pragma warning disable ASPIRECOSMOSDB001

var builder = DistributedApplication.CreateBuilder(args);

var databaseName = builder.Configuration["Cosmos:DatabaseName"] ?? "anime-feed";

var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator => emulator
        .WithDataExplorer()
        .WithImagePullPolicy(ImagePullPolicy.Missing)
        .WithLifetime(ContainerLifetime.Persistent));

cosmos.AddCosmosDatabase(databaseName);

builder.AddProject<Projects.AnimeFeedManager_Web>("web")
    .WithReference(cosmos)
    .WaitFor(cosmos);

builder.Build().Run();
