using System.Security.Claims;
using AnimeFeedManager.Shared.Types;

namespace AnimeFeedManager.Web.Features.Security;

internal static class CustomClaimTypes
{
    internal const string Sub = "sub";
}

internal static class UserProvider
{
    internal static AppUser GetCurrentUser(this IHttpContextAccessor contextAccessor)
    {
        if (contextAccessor.HttpContext?.User.Identity?.IsAuthenticated is null or false)
            return new Anonymous();

        var userId = contextAccessor.HttpContext?.User.FindFirstValue(CustomClaimTypes.Sub);
        var email = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        var role = UserRole.FromString(contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ??
                                       string.Empty);

        return (userId, email, role.IsAdmin()) switch
        {
            (not null, not null, true) => GetAdmin(userId, email),
            (not null, not null, false) => GetAppUser(userId, email),
            _ => new Anonymous()
        };
    }

    private static AppUser GetAppUser(string userId, string email)
    {
        return email.ParseAsEmail()
            .And(userId.ParseAsNonEmpty())
            .AsResult()
            .MatchToValue<AppUser>(r => new RegularUser(
                    r.Item1,
                    r.Item2),
                _ => new Anonymous());
    }

    private static AppUser GetAdmin(string userId, string email)
    {
        return email.ParseAsEmail()
            .And(userId.ParseAsNonEmpty())
            .AsResult()
            .MatchToValue<AppUser>(r => new AdminUser(
                    r.Item1,
                    r.Item2),
                _ => new Anonymous());
    }
}