using System.Security.Claims;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Web.Features.Security;

internal static class CustomClaimTypes
{
    internal const string Sub = "sub";
}

public interface IUserProvider
{
    AppUser GetCurrentUser();
}

public class UserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public UserProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }


    public AppUser GetCurrentUser()
    {
        if (_contextAccessor.HttpContext?.User.Identity?.IsAuthenticated is null or false) return new Anonymous();

        var email = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        var userId = _contextAccessor.HttpContext?.User?.FindFirstValue(CustomClaimTypes.Sub);
        var role = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

        return (email, userId, role) switch
        {
            (not null, not null, RoleNames.User) => GetUser(email, userId),
            (not null, not null, RoleNames.Admin) => GetAdmin(email, userId),
            _ => new Anonymous()
        };
    }

    private AppUser GetUser(string email, string userId)
    {
        return Validate(email, userId)
            .Map(data => new User(data.Email, data.UserId))
            .MatchUnsafe<AppUser>(
                user => user, 
                _ => new Anonymous(), 
                () => new Anonymous());
    }
    
    private AppUser GetAdmin(string email, string userId)
    {
        return Validate(email, userId)
            .Map(data => new AdminUser(data.Email, data.UserId))
            .MatchUnsafe<AppUser>(
                user => user, 
                _ => new Anonymous(), 
                () => new Anonymous());
    }

    private static Either<DomainError, (Email Email, UserId UserId)> Validate(string email, string userId)
    {
        return (EmailValidator.Validate(email), UserIdValidator.Validate(userId))
            .Apply((e, userid) => (email: e, userid))
            .ValidationToEither();
    }
}