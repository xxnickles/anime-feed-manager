namespace AnimeFeedManager.Features.State.Types;

public readonly record struct CurrentState(string Id, int Completed, int Errors, bool ShouldNotify);

public record StateWrap<T>(string StateId, T Payload);