using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Features.Users.IO;
using MediatR;
using Microsoft.Extensions.Logging;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Subscriptions;

public record AutomatedSubscription : INotification;

public sealed class AutomatedSubscriptionHandler : INotificationHandler<AutomatedSubscription>
{
    private readonly IDomainPostman _domainPostman;
    private readonly IUserGetter _userGetter;
    private readonly ILogger<AutomatedSubscriptionHandler> _logger;

    public AutomatedSubscriptionHandler(
        IDomainPostman domainPostman,
        IUserGetter userGetter,
        ILogger<AutomatedSubscriptionHandler> logger)
    {
        _domainPostman = domainPostman;
        _userGetter = userGetter;
        _logger = logger;
    }

    public async Task Handle(AutomatedSubscription _, CancellationToken cancellationToken)
    {
        var results = await _userGetter.GetAvailableUsers(cancellationToken)
            .MapAsync(users => users.ConvertAll(u => new UserAutoSubscription(u)))
            .BindAsync(events => SendMessages(events, cancellationToken));

        results.Match(
            _ => _logger.LogInformation("Automated subscriptions will be processed for available users"),
            error => error.LogDomainError(_logger));
    }

    private async Task<Either<DomainError, Unit>> SendMessages(
        ImmutableList<UserAutoSubscription> events, CancellationToken token)
    {
        return await Task.WhenAll(events.AsParallel()
                .Select(autoSubscriptionEvent =>
                    _domainPostman.SendMessage(autoSubscriptionEvent, Box.UserAutoSubscription, token)))
            .Flatten()
            .MapAsync(_ => unit);
    }
}