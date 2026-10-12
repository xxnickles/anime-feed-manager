namespace AnimeFeedManager.Features.Infrastructure.Messaging;

internal static class MessagingExtensions
{
    extension<T>(Task<Result<T>> result)
    {
        /// <summary>On failure, sends the error events; the original error stays the result and a failed send is logged with it.</summary>
        internal Task<Result<T>> SendOnError(DomainCollectionSender domainPostman, DomainMessage[] errorEvents,
            CancellationToken token) =>
            result.BindOnError(error => domainPostman(errorEvents, token)
                .MatchToValue(
                    _ => Result<T>.Failure(error),
                    sendError => Result<T>.Failure(error).AddLog(sendError.LogAction())));
    }
}
