using System.Text.Json;
using AnimeFeedManager.Features.Common.Domain.Errors;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Features.Infrastructure.Messaging;

public readonly record struct MinutesDelay()
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
    Task<Either<DomainError, Unit>> SendMessage<T>(T message, Box destiny,
        CancellationToken cancellationToken = default);

    Task<Either<DomainError, Unit>> SendDelayedMessage<T>(T message, Box destiny, MinutesDelay delay,
        CancellationToken cancellationToken = default);
}

public class AzureQueueMessages : IDomainPostman
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly QueueClientOptions _queueClientOptions;

    private readonly AzureBlobStorageOptions _blobStorageOptions;

    public AzureQueueMessages(
        IOptionsSnapshot<AzureBlobStorageOptions> blobStorageOptions)
    {
        _blobStorageOptions = blobStorageOptions.Value;
        _jsonOptions = new JsonSerializerOptions(new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _queueClientOptions = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };
    }

    public async Task<Either<DomainError, Unit>> SendMessage<T>(T message, Box destiny,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await SendMessage(message, destiny, null, cancellationToken);
            return unit;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    public async Task<Either<DomainError, Unit>> SendDelayedMessage<T>(T message, Box destiny, MinutesDelay delay,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await SendMessage(message, destiny, TimeSpan.FromMinutes(delay.Value), cancellationToken);
            return unit;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private async Task SendMessage<T>(T message, string destiny, TimeSpan? delay = default,
        CancellationToken cancellationToken = default)
    {
        var queue = new QueueClient(_blobStorageOptions?.StorageConnectionString ?? string.Empty, destiny,
            _queueClientOptions);
        await queue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await queue.SendMessageAsync(AsBinary(message), cancellationToken: cancellationToken, visibilityTimeout: delay);
    }

    private BinaryData AsBinary<T>(T data)
    {
        return BinaryData.FromObjectAsJson(data, _jsonOptions);
    }
}