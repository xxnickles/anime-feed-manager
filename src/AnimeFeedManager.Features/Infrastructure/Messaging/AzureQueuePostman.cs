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

public delegate Task<Result<Unit>> DomainCollectionSender(IEnumerable<DomainMessage> messages,
    CancellationToken cancellationToken = default);


public interface IDomainPostman{

    Task<Result<Unit>> SendMessages<T>(IEnumerable<T> message, CancellationToken cancellationToken = default)
        where T : DomainMessage;

    Task<Result<Unit>> SendDelayedMessage<T>(T message, Delay delay,
        CancellationToken cancellationToken = default) where T : DomainMessage;
}

public class AzureQueuePostman : IDomainPostman
{
    private static readonly ActivitySource Source = new(Telemetry.MessagingSource);

    private readonly QueueServiceClient _queueServiceClient;

    public AzureQueuePostman(QueueServiceClient queueServiceClient)
    {
        _queueServiceClient = queueServiceClient;
    }
   

    public async Task<Result<Unit>> SendMessages<T>(IEnumerable<T> messages,
        CancellationToken cancellationToken = default) where T : DomainMessage
    {
        // If there is only one message, send it directly
        if (messages.Count() is 1)
            return await SendMessage(messages.First(), cancellationToken);
        
        var processMessages = new Queue<T>(messages);
        try
        {
            if (processMessages.Any(m => m.MessageBox.HasNoTarget()))
            {
                return Error.Create("One of the messages has no target box");
            }

            // Dequeue only after a successful send, so the failed message is reported as undelivered
            while (processMessages.Count > 0)
            {
                var message = processMessages.Peek();
                await SendMessage(message, message.MessageBox, null, cancellationToken);
                processMessages.Dequeue();
            }


            return new Unit();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return MessagesNotDelivered.Create(e, processMessages);
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
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return MessagesNotDelivered.Create(e, message);
        }
    }
    
    private async Task<Result<Unit>> SendMessage<T>(T message, CancellationToken cancellationToken = default)
        where T : DomainMessage
    {
        if (message.MessageBox.HasNoTarget())
            return Error.Create($"{typeof(T).FullName} has not a target box");

        try
        {
            await SendMessage(message, message.MessageBox, null, cancellationToken);
            return new Unit();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return MessagesNotDelivered.Create(e, message);
        }
    }

    private async Task SendMessage<T>(T message, string destiny, TimeSpan? delay = default,
        CancellationToken cancellationToken = default) where T : DomainMessage
    {
        using var activity = Source.StartActivity($"{Telemetry.MessagingSendOperation} {destiny}", ActivityKind.Producer);
        activity?.SetTag(Telemetry.MessagingSystemTag, Telemetry.MessagingSystemValue);
        activity?.SetTag(Telemetry.MessagingOperationNameTag, Telemetry.MessagingSendOperation);
        activity?.SetTag(Telemetry.MessagingOperationTypeTag, Telemetry.MessagingSendOperation);
        activity?.SetTag(Telemetry.MessagingDestinationNameTag, destiny);

        // Consumers link to this context: the send span's own, or the caller's when the span isn't sampled
        if ((activity ?? Activity.Current) is { } context)
        {
            message = message with
            {
                TraceInformation = new TraceInformation(context.Id, context.TraceStateString)
            };
        }

        var queue = _queueServiceClient.GetQueueClient(destiny);

        // Convert to base64 string explicitly
        string base64Message = Convert.ToBase64String(message.ToBinaryData());

        try
        {
            var receipt = await queue.SendMessageAsync(base64Message, cancellationToken: cancellationToken,
                visibilityTimeout: delay);
            activity?.SetTag(Telemetry.MessagingMessageIdTag, receipt.Value.MessageId);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // Logged once by the caller's MessagesNotDelivered; the span only records the failure
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            activity?.SetTag(Telemetry.ErrorTypeTag, e.GetType().FullName);
            throw;
        }
    }
}