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

public delegate Task<Result<Unit, DeliveryError>> DomainCollectionSender(IEnumerable<DomainMessage> messages,
    CancellationToken cancellationToken = default);

public delegate Task<Result<Unit, DeliveryError>> DelayedMessageSender(DomainMessage message, Delay delay,
    CancellationToken cancellationToken = default);


public interface IDomainPostman{

    Task<Result<Unit, DeliveryError>> SendMessages<T>(IEnumerable<T> message, CancellationToken cancellationToken = default)
        where T : DomainMessage;

    Task<Result<Unit, DeliveryError>> SendDelayedMessage<T>(T message, Delay delay,
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
   

    public async Task<Result<Unit, DeliveryError>> SendMessages<T>(IEnumerable<T> messages,
        CancellationToken cancellationToken = default) where T : DomainMessage
    {
        // If there is only one message, send it directly
        if (messages.Count() is 1)
            return await SendMessage(messages.First(), cancellationToken);
        
        var processMessages = new Queue<T>(messages);
        try
        {
            if (processMessages.FirstOrDefault(m => m.MessageBox.HasNoTarget()) is { } untargeted)
                return new DeliveryError.NoTargetBox(untargeted.GetType());

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
            return new DeliveryError.NotDelivered([.. processMessages], e);
        }
    }

    public async Task<Result<Unit, DeliveryError>> SendDelayedMessage<T>(T message, Delay delay,
        CancellationToken cancellationToken = default) where T : DomainMessage
    {
        if (message.MessageBox.HasNoTarget())
            return new DeliveryError.NoTargetBox(message.GetType());

        try
        {
            await SendMessage(message, message.MessageBox, delay.Value, cancellationToken);
            return new Unit();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return new DeliveryError.NotDelivered([message], e);
        }
    }
    
    private async Task<Result<Unit, DeliveryError>> SendMessage<T>(T message, CancellationToken cancellationToken = default)
        where T : DomainMessage
    {
        if (message.MessageBox.HasNoTarget())
            return new DeliveryError.NoTargetBox(message.GetType());

        try
        {
            await SendMessage(message, message.MessageBox, null, cancellationToken);
            return new Unit();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return new DeliveryError.NotDelivered([message], e);
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
        activity?.SetTag(Telemetry.ServerAddressTag, _queueServiceClient.Uri.Host);

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
            // Logged once by the caller's DeliveryError.NotDelivered; the span only records the failure
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            activity?.SetTag(Telemetry.ErrorTypeTag, e.GetType().FullName);
            throw;
        }
    }
}