namespace AnimeFeedManager.Features.Infrastructure.Messaging;

// internal static class Extensions
// {
//     internal static Task<Result<T>> SendDomainMessages<T>(this IDomainPostman domainPostman,
//         IEnumerable<DomainMessage> messages,
//         IEnumerable<DomainMessage> errorMessages,
//         CancellationToken token)
//     {
//         return result
//             .Bind(r => domainPostman.SendMessages(messages, token).Map(_ => r))
//             .MapError(r => domainPostman.SendMessages(errorMessages, token)
//                 .MatchToValue(_ => r, error => error)
//             );
//     }
//
//
//     internal static Task<Result<T>> SendDomainMessages<T>(this Task<Result<T>> result, IDomainPostman domainPostman,
//         DomainMessage messages,
//         DomainMessage errorMessages,
//         CancellationToken token)
//     {
//         return result
//             .Bind(r => domainPostman.SendMessage(messages, token).Map(_ => r))
//             .MapError(r => domainPostman.SendMessage(errorMessages, token)
//                 .MatchToValue(_ => r, error => error)
//             );
//     }
//}

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
