using System.Text.Json;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using Azure.Storage.Queues;

namespace AnimeFeedManager.Features.Infrastructure.Messaging;

public readonly record struct Delay
{
    public TimeSpan Value { get; } = TimeSpan.Zero;

    public Delay(TimeSpan value) 
    {
        if (value.Days >= 7)
        {
            throw new ArgumentException(
                "Value cannot exceed 10,000 minutes (~7 days) as per infrastructure limitations");
        }

        Value = value;
    }
}

public interface IDomainPostman
{
    Task<Either<DomainError, Unit>> SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : DomainMessage;

    Task<Either<DomainError, Unit>> SendDelayedMessage<T>(T message, Delay delay,
        CancellationToken cancellationToken = default) where T : DomainMessage;
}

public class AzureQueueMessages : IDomainPostman
{
    private AzureStorageSettings _azureSettings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly QueueClientOptions _queueClientOptions;

    public AzureQueueMessages(AzureStorageSettings tableStorageSettings)
    {
        _azureSettings = tableStorageSettings;
        _jsonOptions = new JsonSerializerOptions(new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _queueClientOptions = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };
    }

    public async Task<Either<DomainError, Unit>> SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : DomainMessage
    {
        if (message.MessageBox.HasNoTarget()) return new BasicError($"{typeof(T).FullName} has not a target box");
        
        try
        {
            await SendMessage(message, message.MessageBox, null, cancellationToken);
            return unit;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    public async Task<Either<DomainError, Unit>> SendDelayedMessage<T>(T message, Delay delay,
        CancellationToken cancellationToken = default) where T : DomainMessage
    {
        if (message.MessageBox.HasNoTarget()) return new BasicError($"{typeof(T).FullName} has not a target box");
        
        try
        {
            await SendMessage(message, message.MessageBox, delay.Value, cancellationToken);
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
        var queue = GetClient(destiny);
        await queue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await queue.SendMessageAsync(AsBinary(message), cancellationToken: cancellationToken, visibilityTimeout: delay);
    }

    private QueueClient GetClient(string destiny)
    {
        return _azureSettings switch
        {
            ConnectionStringSettings connectionStringOptions => new QueueClient(
                connectionStringOptions.StorageConnectionString, destiny,
                _queueClientOptions),
            TokenCredentialSettings tokenCredentialOptions => new QueueClient(
                new Uri(tokenCredentialOptions.QueueUri, destiny), tokenCredentialOptions.DefaultTokenCredential(), _queueClientOptions),
            _ => throw new ArgumentException(
                "Provided Table Storage configuration is not valid. Make sure Configurations for Azure table Storage is correct for either connection string or managed identities",
                nameof(TableClientOptions))
        };
    }

    private BinaryData AsBinary<T>(T data)
    {
        return BinaryData.FromObjectAsJson(data, _jsonOptions);
    }
}