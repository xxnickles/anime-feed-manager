using System.Security.Claims;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.IO;

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
    private AppUser? _cachedUser; 
    
    private readonly record struct UserData(
        Email Email,
        UserId UserId,
        TvSubscriptions TvSubscriptions,
        OvaSubscriptions OvaSubscriptions,
        MovieSubscriptions MovieSubscriptions);

    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IGetTvSubscriptions _getTvSubscriptions;
    private readonly IGetInterestedSeries _getInterestedSeries;
    private readonly IGetOvasSubscriptions _ovasSubscriptions;
    private readonly IGetMovieSubscriptions _movieSubscriptions;

    public UserProvider(
        IHttpContextAccessor contextAccessor,
        IGetTvSubscriptions getTvSubscriptions,
        IGetInterestedSeries getInterestedSeries,
        IGetOvasSubscriptions ovasSubscriptions,
        IGetMovieSubscriptions movieSubscriptions)
    {
        _contextAccessor = contextAccessor;
        _getTvSubscriptions = getTvSubscriptions;
        _getInterestedSeries = getInterestedSeries;
        _ovasSubscriptions = ovasSubscriptions;
        _movieSubscriptions = movieSubscriptions;
    }

    public async Task<AppUser> GetCurrentUser(CancellationToken token)
    {
        if (_cachedUser is not null) return _cachedUser;
        
        if (_contextAccessor.HttpContext?.User.Identity?.IsAuthenticated is null or false)
            return new Anonymous();

        var userId = _contextAccessor.HttpContext?.User.FindFirstValue(CustomClaimTypes.Sub);
        var role = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

        var result = (userId, role) switch
        {
            (not null, RoleNames.User) => GetUser(userId, token),
            (not null, RoleNames.Admin) => GetAdmin(userId, token),
            _ => Task.FromResult<AppUser>(new Anonymous())
        };
        
        _cachedUser = await result;
        return _cachedUser;
    }

    private async Task<AppUser> GetUser(string userId, CancellationToken token)
    {
        var process = await UserId.Parse(userId)
            .BindAsync(id => AddSubscriptions(id, token))
            .MapAsync(data => new User(data.Email, data.UserId, data.TvSubscriptions, data.OvaSubscriptions, data.MovieSubscriptions));

        return process.MatchUnsafe<AppUser>(
            user => user,
            _ => new Anonymous(),
            () => new Anonymous());
    }

    private async Task<AppUser> GetAdmin(string userId, CancellationToken token)
    {
        var process = await UserId.Parse(userId)
            .BindAsync(id => AddSubscriptions(id, token))
            .MapAsync(data => new AdminUser(data.Email, data.UserId, data.TvSubscriptions, data.OvaSubscriptions,
                data.MovieSubscriptions));

        return process.MatchUnsafe<AppUser>(
            user => user,
            _ => new Anonymous(),
            () => new Anonymous());
    }

    private Task<Either<DomainError, UserData>> AddSubscriptions(
        UserId userId, CancellationToken token)
    {
        return _getTvSubscriptions.GetUserSubscriptions(userId, token)
            .BindAsync(subscriptions => _getInterestedSeries.Get(userId, token)
                .MapAsync(interested =>
                    (TvSubscriptions: new TvSubscriptions(subscriptions.Series.ConvertAll(x => x.ToString()),
                        interested.ConvertAll(x => x.RowKey ?? string.Empty)), subscriptions.SubscriberEmail)))
            .BindAsync(data =>
                _ovasSubscriptions.GetSubscriptions(userId, token)
                    .MapAsync(ovasSubs => (data.SubscriberEmail, data.TvSubscriptions, new OvaSubscriptions(ovasSubs))))
            .BindAsync(data =>
                _movieSubscriptions.GetSubscriptions(userId, token)
                    .MapAsync(moviesSubs =>
                        new UserData(data.SubscriberEmail, userId, data.TvSubscriptions, data.Item3,
                            new MovieSubscriptions(moviesSubs))));
    }
}