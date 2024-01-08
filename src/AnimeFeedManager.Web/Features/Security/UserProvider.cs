using System.Collections.Immutable;
using System.Security.Claims;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;

namespace AnimeFeedManager.Web.Features.Security;

internal static class CustomClaimTypes
{
    internal const string Sub = "sub";
}

public interface IUserProvider
{
    Task<AppUser> GetCurrentUser(CancellationToken token);
}

public class UserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IGetTvSubscriptions _getTvSubscriptions;

    public UserProvider(
        IHttpContextAccessor contextAccessor,
        IGetTvSubscriptions getTvSubscriptions)
    {
        _contextAccessor = contextAccessor;
        _getTvSubscriptions = getTvSubscriptions;
    }

    public Task<AppUser> GetCurrentUser(CancellationToken token)
    {
        if (_contextAccessor.HttpContext?.User.Identity?.IsAuthenticated is null or false)
            return Task.FromResult<AppUser>(new Anonymous());
        
        var userId = _contextAccessor.HttpContext?.User?.FindFirstValue(CustomClaimTypes.Sub);
        var role = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

        return (userId, role) switch
        {
            (not null, RoleNames.User) => GetUser(userId, token),
            (not null, RoleNames.Admin) => GetAdmin(userId, token),
            _ => Task.FromResult<AppUser>(new Anonymous())
        };
    }

    private async Task<AppUser> GetUser(string userId, CancellationToken token)
    {
        var process = await Validate(userId)
            .BindAsync(id => AddSubscriptions(id, token))
            .MapAsync(data => new AdminUser(data.Email, data.UserId, data.Subscriptions));

        return process.MatchUnsafe<AppUser>(
            user => user,
            _ => new Anonymous(),
            () => new Anonymous());
    }

    private async Task<AppUser> GetAdmin(string userId, CancellationToken token)
    {
        var process = await Validate(userId)
            .BindAsync(id => AddSubscriptions(id, token))
            .MapAsync(data => new AdminUser(data.Email, data.UserId, data.Subscriptions));

        return process.MatchUnsafe<AppUser>(
            user => user,
            _ => new Anonymous(),
            () => new Anonymous());
    }

    private Task<Either<DomainError, (Email Email, UserId UserId, ImmutableList<NoEmptyString> Subscriptions)>>
        AddSubscriptions(UserId userId, CancellationToken token)
    {
        return _getTvSubscriptions.GetUserSubscriptions(userId, token)
            .MapAsync(subscriptions => (subscriptions.SubscriberEmail, userId, subscriptions.Series));
    }

    private static Either<DomainError, UserId> Validate(string userId)
    {
        return UserIdValidator.Validate(userId)
            .ValidationToEither();
    }
}