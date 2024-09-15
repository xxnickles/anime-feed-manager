using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Features.Users.IO;
using Microsoft.Extensions.Logging;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Subscriptions;

public sealed class AutomatedSubscriptionHandler(
    IDomainPostman domainPostman,
    IUserGetter userGetter,
    ILogger<AutomatedSubscriptionHandler> logger) {
    public async Task Handle(AutomatedSubscription _, CancellationToken cancellationToken)
    {
        var results = await userGetter.GetAvailableUsers(cancellationToken)
            .MapAsync(users => users.ConvertAll(u => new UserAutoSubscription(u)))
            .BindAsync(events => SendMessages(events, cancellationToken));

        results.Match(
            _ => logger.LogInformation("Automated subscriptions will be processed for available users"),
            error => error.LogError(logger));
    }

    private async Task<Either<DomainError, Unit>> SendMessages(
        ImmutableList<UserAutoSubscription> events, CancellationToken token)
    {
        return await Task.WhenAll(events.AsParallel()
                .Select(autoSubscriptionEvent =>
                    domainPostman.SendMessage(autoSubscriptionEvent, token)))
            .FlattenResults()
            .MapAsync(_ => unit);
    }
}