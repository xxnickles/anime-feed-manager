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