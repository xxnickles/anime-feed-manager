using System.Security.Claims;
using AnimeFeedManager.Shared.Results.Static;
using AnimeFeedManager.Shared.Types;

namespace AnimeFeedManager.Web.Features.Security;

internal static class CustomClaimTypes
{
    internal const string Sub = "sub";
}

internal static class UserProvider
{
    /// <summary>
    /// Projects the current request's <see cref="ClaimsPrincipal"/> into the domain
    /// <see cref="AppUser"/>. Unauthenticated or malformed claims degrade to <see cref="Anonymous"/>.
    /// </summary>
    internal static AppUser GetCurrentUser(this HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated is null or false)
            return new Anonymous();

        var userId = context.User.FindFirstValue(CustomClaimTypes.Sub);
        var email = context.User.FindFirstValue(ClaimTypes.Email);
        var role = UserRole.FromString(context.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty);

        return (userId, email, role.IsAdmin()) switch
        {
            (not null, not null, true) => Build(email, userId, (e, id) => new AdminUser(e, id)),
            (not null, not null, false) => Build(email, userId, (e, id) => new RegularUser(e, id)),
            _ => new Anonymous()
        };
    }

    private static AppUser Build(string email, string userId, Func<Email, NoEmptyString, AppUser> create) =>
        email.ParseAsEmail().AsResult()
            .Bind(e => userId.ParseAsNonEmpty().AsResult().Map(id => (Email: e, UserId: id)))
            .MatchToValue(parts => create(parts.Email, parts.UserId), _ => new Anonymous());
}
