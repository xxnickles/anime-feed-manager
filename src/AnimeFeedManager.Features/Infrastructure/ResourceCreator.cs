using AnimeFeedManager.Features.Images;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;

namespace AnimeFeedManager.Features.Infrastructure;

public class ResourceCreator(
    BlobServiceClient blobServiceClient,
    QueueServiceClient queueServiceClient,
    TableServiceClient tableServiceClient,
    ILogger<ResourceCreator> logger)
{
    
    public Task TryCreateResources(CancellationToken cancellationToken)
    {
        Task[] tasks =
        [
            CreateQueues(cancellationToken),
            CreateTables(cancellationToken),
            CreateContainers(cancellationToken)
        ];

        return Task.WhenAll(tasks);
    }
    
    private async Task CreateContainers(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating container {blob.container}", ImageProvider.Container);
        
            var containerClient = blobServiceClient.GetBlobContainerClient(ImageProvider.Container);
            var response = await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        
            if (response is null || !response.HasValue)
            {
                logger.LogInformation("Container {blob.container} already exists", ImageProvider.Container);
                return;
            }

            var accessPolicy = await containerClient.GetAccessPolicyAsync(cancellationToken: cancellationToken);
            if (accessPolicy.Value.BlobPublicAccess is not PublicAccessType.Blob)
            {
                await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
            }

            logger.LogInformation("Container {blob.container} created", ImageProvider.Container);

        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError(e, "An error occurred when creating container {blob.container}", ImageProvider.Container);
        }
    }
    
    private Task CreateQueues(CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating queues");
        var results = DomainMessageQueues.AllQueues
            .Select(queue => CreateQueue(queueServiceClient, queue, logger, cancellationToken))
            .ToArray();

        return Task.WhenAll(results);
    }

    private static async Task CreateQueue(
        QueueServiceClient serviceClient,
        string queueName,
        ILogger<ResourceCreator> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await serviceClient.CreateQueueAsync(queueName, cancellationToken: cancellationToken);
            if (response.HasValue)
            {
                logger.LogInformation("Queue {messaging.destination.name} created", response.Value.Name);
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError(e, "An error occurred when creating queue {messaging.destination.name}", queueName);
        }
    }

    private Task CreateTables(CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating tables");
        var results = AzureTableName.AllTableNames
            .Select(table => CreateTable(tableServiceClient, table, logger, cancellationToken))
            .ToArray();

        return Task.WhenAll(results);
    }

    private static async Task CreateTable(
        TableServiceClient serviceClient,
        string tableName,
        ILogger<ResourceCreator> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var response =
                await serviceClient.CreateTableIfNotExistsAsync(tableName, cancellationToken: cancellationToken);
            if (response.HasValue)
            {
                logger.LogInformation("Table {table.name} created", response.Value.Name);
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError(e, "An error occurred when creating table {table.name}", tableName);
        }
    }
}