using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Users.IO;
using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Users;

public class SubscriptionCopierSetter
{
    private readonly IUserVerification _userVerification;
    private readonly IDomainPostman _domainPostman;

    public SubscriptionCopierSetter(
        IUserVerification userVerification,
        IDomainPostman domainPostman)
    {
        _userVerification = userVerification;
        _domainPostman = domainPostman;
    }

    public Task<Either<DomainError, UsersCheck>> StartCopyProcess(UserId source, UserId target,
        CancellationToken token = default)
    {
        return _userVerification.CheckUsersExist(token, source, target)
            .BindAsync(result => TrySendNotification(result, source, target, token));
    }


    private Task<Either<DomainError, UsersCheck>> TrySendNotification(UsersCheck result, UserId source, UserId target,
        CancellationToken token)
    {
        return result is AllMatched
            ? _domainPostman.SendMessage(new CopySubscriptionRequest(source, target), token)
                .MapAsync(_ => result)
            : Task.FromResult(Right<DomainError, UsersCheck>(result));
    }
}