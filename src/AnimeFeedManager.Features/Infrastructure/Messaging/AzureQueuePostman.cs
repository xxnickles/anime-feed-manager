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
    private readonly QueueServiceClient _queueServiceClient;
    private readonly ILogger<AzureQueuePostman> _logger;

    public AzureQueuePostman(
        QueueServiceClient queueServiceClient,
        ILogger<AzureQueuePostman> logger)
    {
        _queueServiceClient = queueServiceClient;
        _logger = logger;
    }

    public async Task<Result<Unit>> SendMessage<T>(T message, CancellationToken cancellationToken = default)
        where T : DomainMessage
    {
        if (message.MessageBox.HasNoTarget())
            return Error.Create($"{typeof(T).FullName} has not a target box");

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
                return Error.Create("One of the messages has no target box");
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
            return Error.Create($"{typeof(T).FullName} has not a target box");

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
        CancellationToken cancellationToken = default) where T : DomainMessage
    {
        // Capture current activity and add the info to the message. It can be useful for tracing in the consumer functions
        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            message = message with
            {
                TraceInformation = new TraceInformation(currentActivity.Id, currentActivity.TraceStateString)
            };
        }

        var queue = _queueServiceClient.GetQueueClient(destiny);

        // Convert to base64 string explicitly
        string base64Message = Convert.ToBase64String(message.ToBinaryData());

        await queue.SendMessageAsync(base64Message, cancellationToken: cancellationToken, visibilityTimeout: delay);
    }
}