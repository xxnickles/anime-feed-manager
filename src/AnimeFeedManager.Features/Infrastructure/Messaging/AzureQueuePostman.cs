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
    Task<Result<Unit>> SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : DomainMessage;

    Task<Result<Unit>> SendMessages<T>(IEnumerable<T> message, CancellationToken cancellationToken = default)
        where T : DomainMessage;

    Task<Result<Unit>> SendDelayedMessage<T>(T message, Delay delay,
        CancellationToken cancellationToken = default) where T : DomainMessage;
}

public class AzureQueuePostman : IDomainPostman
{
    private readonly AzureStorageSettings _azureSettings;
    private readonly ILogger<AzureQueuePostman> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly QueueClientOptions _queueClientOptions;

    public AzureQueuePostman(
        AzureStorageSettings tableStorageSettings,
        ILogger<AzureQueuePostman> logger)
    {
        _azureSettings = tableStorageSettings;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions(new JsonSerializerOptions
            {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        _queueClientOptions = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };
    }

    public async Task<Result<Unit>> SendMessage<T>(T message, CancellationToken cancellationToken = default)
        where T : DomainMessage
    {
        if (message.MessageBox.HasNoTarget())
            return new Error($"{typeof(T).FullName} has not a target box");

        try
        {
            await SendMessage(message, message.MessageBox, null, cancellationToken);
            return new Unit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending message {Message}", message);
            return MessagesNotDelivered.Create(e.Message, message);
        }
    }

    public async Task<Result<Unit>> SendMessages<T>(IEnumerable<T> messages,
        CancellationToken cancellationToken = default) where T : DomainMessage
    {
        var processMessages = new Queue<T>(messages);
        try
        {
            if (processMessages.Any(m => m.MessageBox.HasNoTarget()))
            {
                return new Error("One of the messages has no target box");
            }

            while (processMessages.Count > 0)
            {
                var message = processMessages.Dequeue();
                await SendMessage(message, message.MessageBox, null, cancellationToken);
            }


            return new Unit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An occurred when trying to send multiple messages to {Queues}",
                string.Join(", ", processMessages.Select(m => m.MessageBox)));
            return MessagesNotDelivered.Create(e.Message, processMessages);
        }
    }

    public async Task<Result<Unit>> SendDelayedMessage<T>(T message, Delay delay,
        CancellationToken cancellationToken = default) where T : DomainMessage
    {
        if (message.MessageBox.HasNoTarget())
            return new Error($"{typeof(T).FullName} has not a target box");

        try
        {
            await SendMessage(message, message.MessageBox, delay.Value, cancellationToken);
            return new Unit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending message {Message}", message);
            return MessagesNotDelivered.Create(e.Message, message);
        }
    }

    private async Task SendMessage<T>(T message, string destiny, TimeSpan? delay = default,
        CancellationToken cancellationToken = default)
    {
        var queue = GetClient(destiny);
        await queue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await queue.SendMessageAsync(AsBinary(message), cancellationToken: cancellationToken, visibilityTimeout: delay);
    }

    private QueueClient GetClient(string destinatary)
    {
        return _azureSettings switch
        {
            ConnectionStringSettings connectionStringOptions => new QueueClient(
                connectionStringOptions.StorageConnectionString, destinatary,
                _queueClientOptions),
            TokenCredentialSettings tokenCredentialOptions => new QueueClient(
                new Uri(tokenCredentialOptions.QueueUri, destinatary), tokenCredentialOptions.DefaultTokenCredential(),
                _queueClientOptions),
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