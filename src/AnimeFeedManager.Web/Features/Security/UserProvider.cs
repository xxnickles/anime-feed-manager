using System.Security.Claims;
using AnimeFeedManager.Common.Domain.Types;
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
    private readonly IGetInterestedSeries _getInterestedSeries;

    public UserProvider(
        IHttpContextAccessor contextAccessor,
        IGetTvSubscriptions getTvSubscriptions,
        IGetInterestedSeries getInterestedSeries)
    {
        _contextAccessor = contextAccessor;
        _getTvSubscriptions = getTvSubscriptions;
        _getInterestedSeries = getInterestedSeries;
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
        var process = await UserId.Parse(userId)
            .BindAsync(id => AddSubscriptions(id, token))
            .MapAsync(data => new User(data.Email, data.UserId, data.TvSubscriptions));

        return process.MatchUnsafe<AppUser>(
            user => user,
            _ => new Anonymous(),
            () => new Anonymous());
    }

    private async Task<AppUser> GetAdmin(string userId, CancellationToken token)
    {
        var process = await UserId.Parse(userId)
            .BindAsync(id => AddSubscriptions(id, token))
            .MapAsync(data => new AdminUser(data.Email, data.UserId, data.TvSubscriptions));

        return process.MatchUnsafe<AppUser>(
            user => user,
            _ => new Anonymous(),
            () => new Anonymous());
    }

    private Task<Either<DomainError, (Email Email, UserId UserId, TvSubscriptions TvSubscriptions)>> AddSubscriptions(
        UserId userId, CancellationToken token)
    {
        return _getTvSubscriptions.GetUserSubscriptions(userId, token)
            .BindAsync(subscriptions => _getInterestedSeries.Get(userId, token)
                .MapAsync(interested =>
                    (TvSubscriptions: new TvSubscriptions(subscriptions.Series.ConvertAll(x => x.ToString()),
                        interested.ConvertAll(x => x.RowKey ?? string.Empty)), subscriptions.SubscriberEmail)))
            .MapAsync(data => (data.SubscriberEmail, userId, data.TvSubscriptions));
    }
}