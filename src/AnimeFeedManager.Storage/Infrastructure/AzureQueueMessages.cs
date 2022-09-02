using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Storage.Infrastructure;

public record struct MinutesDelay()
{
    public ushort Value { get; } = 0;

    public MinutesDelay(ushort value) : this()
    {
        if (value > 10000)
        {
            throw new ArgumentException(
                "Value cannot exceed 10,000 minutes (~7 days) as per infrastructure limitations");
        }

        Value = value;
    }
}

public interface IDomainPostman
{
    Task SendMessage<T>(T message, CancellationToken cancellationToken = default);

    Task SendDelayedMessage<T>(T message, MinutesDelay delay,
        CancellationToken cancellationToken = default);
}

public class AzureQueueMessages : IDomainPostman
{
    private readonly IQueueResolver _queueResolver;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly QueueClientOptions queueClientOptions;

    private readonly AzureBlobStorageOptions _blobStorageOptions;

    public AzureQueueMessages(
        IQueueResolver queueResolver,
        IOptionsSnapshot<AzureBlobStorageOptions> blobStorageOptions)
    {
        _queueResolver = queueResolver;
        _blobStorageOptions = blobStorageOptions.Value;
        jsonOptions = new JsonSerializerOptions(new JsonSerializerOptions
            {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        queueClientOptions = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };
    }

    public Task SendMessage<T>(T message, CancellationToken cancellationToken = default)
    {
        return SendMessage(message, null, cancellationToken);
    }

    public Task SendDelayedMessage<T>(T message, MinutesDelay delay,
        CancellationToken cancellationToken = default)
    {
        return SendMessage(message, TimeSpan.FromMinutes(delay.Value), cancellationToken);
    }

    private async Task SendMessage<T>(T message, TimeSpan? delay = default,
        CancellationToken cancellationToken = default)
    {
        var queue = new QueueClient(_blobStorageOptions?.StorageConnectionString ?? string.Empty, _queueResolver.GetQueue(typeof(T)), queueClientOptions);
        await queue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await queue.SendMessageAsync(AsBinary(message), cancellationToken: cancellationToken, visibilityTimeout: delay);
    }

    private BinaryData AsBinary<T>(T data)
    {
        return BinaryData.FromObjectAsJson(data, jsonOptions);
    }
    
    private static string AsJson<T>(T data)
    {
        return JsonSerializer.Serialize(data,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }
}