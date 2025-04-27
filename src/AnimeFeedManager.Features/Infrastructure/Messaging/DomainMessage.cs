namespace AnimeFeedManager.Features.Infrastructure.Messaging;

/// <summary>
/// Records derived from this abstract are meant to be serialized to be sent to Azure queues
/// </summary>
/// <param name="MessageBox">The Target box</param>
public abstract record DomainMessage(Box MessageBox);