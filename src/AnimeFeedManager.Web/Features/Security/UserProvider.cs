using System.Security.Claims;
using AnimeFeedManager.Shared.Types;

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
    private AppUser? _cachedUser;

    public UserProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public async Task<AppUser> GetCurrentUser(CancellationToken token)
    {
        if (_cachedUser is not null) return _cachedUser;

        if (_contextAccessor.HttpContext?.User.Identity?.IsAuthenticated is null or false)
            return new Anonymous();

        var userId = _contextAccessor.HttpContext?.User.FindFirstValue(CustomClaimTypes.Sub);
        var email = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        var role = UserRole.FromString(_contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ??
                                       string.Empty);

        var result = (userId, email, role.IsAdmin()) switch
        {
            (not null, not null, true) => await GetAdmin(userId, email, token),
            (not null, not null, false) => await GetAppUser(userId, email, token),
            _ => new Anonymous()
        };

        return _cachedUser = result;
    }

    private Task<AppUser> GetAppUser(string userId, string email, CancellationToken token)
    {
        // Temporal while we write the code for getting subscriptions 
        var user = email.ParseAsEmail()
            .And(userId.ParseAsNonEmpty())
            .AsResult()
            .MatchToValue<AppUser>(r => new RegularUser(
                    r.Item1,
                    r.Item2,
                    new TvSubscriptions([], []),
                    new OvaSubscriptions([]),
                    new MovieSubscriptions([])),
                _ => new Anonymous());

        return Task.FromResult(user);
    }

    private Task<AppUser> GetAdmin(string userId, string email, CancellationToken token)
    {
        // Temporal while we write the code for getting subscriptions 
        var user = email.ParseAsEmail()
            .And(userId.ParseAsNonEmpty())
            .AsResult()
            .MatchToValue<AppUser>(r => new AdminUser(
                    r.Item1,
                    r.Item2,
                    new TvSubscriptions([], []),
                    new OvaSubscriptions([]),
                    new MovieSubscriptions([])),
                _ => new Anonymous());

        return Task.FromResult(user);
    }
}