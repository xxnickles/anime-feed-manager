using System.Collections.Immutable;
using System.Net;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Types;
using AnimeFeedManager.WebApp.Services;
using AnimeFeedManager.WebApp.Services.Movies;
using AnimeFeedManager.WebApp.Services.Ovas;
using AnimeFeedManager.WebApp.Services.Tv;
using MudBlazor;

namespace AnimeFeedManager.WebApp.State;

public readonly record struct UserInformation(
    string UserId,
    string UserName,
    bool IsAdmin
);

public sealed class UserSideEffects
{
    private record SystemUser(string UserId);

    private record Invalid(string UserId) : SystemUser(UserId);

    private record FromInput(string UserId, string Email) : SystemUser(UserId);

    private record FromUserName(string UserId, string Email) : SystemUser(UserId);

    private readonly ITvSubscriberService _tvSubscriberService;
    private readonly IOvasSubscriberService _ovasSubscriberService;
    private readonly IMoviesSubscriberService _moviesSubscriberService;
    private readonly IUserService _userService;

    public UserSideEffects(
        ITvSubscriberService tvSubscriberService,
        IOvasSubscriberService ovasSubscriberService,
        IMoviesSubscriberService moviesSubscriberService,
        IUserService userService)
    {
        _tvSubscriberService = tvSubscriberService;
        _ovasSubscriberService = ovasSubscriberService;
        _moviesSubscriberService = moviesSubscriberService;
        _userService = userService;
    }


    public static void CompleteDefaultProfile(ApplicationState state, User user)
    {
        state.SetUser(user);
        state.SetSubscriptions(ImmutableList<string>.Empty);
        state.SetInterested(ImmutableList<string>.Empty);
        state.SetOvasSubscriptions(ImmutableList<string>.Empty);
        state.SetMoviesSubscriptions(ImmutableList<string>.Empty);
    }

    public async Task CompleteUserProfile(
        ApplicationState state,
        UserInformation userInformation,
        Func<Task<string>> localEmailProvider,
        CancellationToken token)
    {
        var email = await GetCurrentUser(
            state,
            userInformation.UserId,
            userInformation.UserName,
            localEmailProvider,
            token);

        var user = await GetUser(state, email, userInformation.IsAdmin, token);
        state.SetUser(user);

        var task = user switch
        {
            AdminUser au => CompleteProfile(state, au.Id, token),
            ApplicationUser ap => CompleteProfile(state, ap.Id, token),
            _ => Task.CompletedTask
        };

        await task;
    }

    private async Task<SystemUser> GetCurrentUser(
        ApplicationState state,
        string userId,
        string userName,
        Func<Task<string>> localEmailProvider,
        CancellationToken token)
    {
        var key = $"email_{userId}";
        state.AddLoadingItem(key, "Loading user Email");
        try
        {
            if ( await _userService.UserExist(userId, token))
            {
                return new SystemUser(userId);
            }
            else
            {
                if (Email.IsEmail(userName))
                {
                    return new FromUserName(userId, userName);
                }
            }

            return await GetEmailFromInput(userId, localEmailProvider);
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.NotFound)
        {
            return await GetEmailFromInput(userId, localEmailProvider);
        }
        catch (Exception ex)
        {
            state.ReportException(new AppException("User Fetching", ex));
            return new Invalid(userId);
        }
        finally
        {
            state.RemoveLoadingItem(key);
        }
    }

    private Task<User> GetUser(ApplicationState state, SystemUser systemUser, bool isAdmin, CancellationToken token)
    {
        return systemUser switch
        {
            FromInput fromInput => TryPersistUser(state, fromInput.UserId, fromInput.Email, isAdmin, token),
            FromUserName fromUserName => TryPersistUser(state, fromUserName.UserId, fromUserName.Email, isAdmin, token),
            Invalid invalid => Task.FromResult<User>(new AuthenticatedUser(invalid.UserId)),
            not null => isAdmin
                ? Task.FromResult<User>(new AdminUser(systemUser.UserId))
                : Task.FromResult<User>(new ApplicationUser(systemUser.UserId)),
            _ => Task.FromResult<User>(new AnonymousUser()) // to be here something went badly wrong
        };
    }

    private async Task<User> TryPersistUser(ApplicationState state, string userId, string email, bool isAdmin,
        CancellationToken token)
    {
        var key = $"save_{userId}";
        state.AddLoadingItem(key, "Storing User");
        try
        {
            await _userService.MergeUser(new SimpleUser(userId, email), token);
            state.ReportNotification(new AppNotification("Email has been stored", Severity.Info));
            state.RemoveLoadingItem(key);
            return isAdmin ? new AdminUser(email) : new ApplicationUser(email);
        }
        catch (Exception ex)
        {
            state.ReportException(new AppException("User Fetching", ex));
            state.RemoveLoadingItem(key);
            return new AuthenticatedUser(userId);
        }
    }

    private static async Task<SystemUser> GetEmailFromInput(string userId, Func<Task<string>> localEmailProvider)
    {
        var emailValue = await localEmailProvider();
        return Email.IsEmail(emailValue) ? new FromInput(userId, emailValue) : new Invalid(userId);
    }

    private async Task CompleteProfile(ApplicationState state, string emailValue, CancellationToken token)
    {
        await GetTvSubscriptions(state, emailValue, token);
        await GetTvInterested(state, emailValue, token);
        await GetOvasSubscriptions(state, emailValue, token);
        await GetMoviesSubscriptions(state, emailValue, token);
        state.ReportNotification(new AppNotification("Profile has been completed", Severity.Info));
    }

    private async Task GetTvInterested(ApplicationState state, string emailValue, CancellationToken token)
    {
        try
        {
            var interested = await _tvSubscriberService.GetInterested(emailValue, token);
            state.SetInterested(interested);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            state.SetSubscriptions(ImmutableList<string>.Empty);
        }
        catch (Exception ex)
        {
            state.ReportException(new AppException("Getting Interested Series", ex));
        }
    }

    private async Task GetTvSubscriptions(ApplicationState state, string emailValue, CancellationToken token)
    {
        try
        {
            var subscriptions = await _tvSubscriberService.GetSubscriptions(emailValue, token);
            state.SetSubscriptions(subscriptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) 
        {
            state.SetSubscriptions(ImmutableList<string>.Empty);
        }
        catch (Exception ex)
        {
            state.ReportException(new AppException("Getting Subscriptions", ex));
        }
    }
    
    private async Task GetOvasSubscriptions(ApplicationState state, string emailValue, CancellationToken token)
    {
        try
        {
            var subscriptions = await _ovasSubscriberService.GetSubscriptions(emailValue, token);
            state.SetOvasSubscriptions(subscriptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            state.SetOvasSubscriptions(ImmutableList<string>.Empty);
        }
        catch (Exception ex)
        {
            state.ReportException(new AppException("Getting Ovas Subscriptions", ex));
        }
    }
    
    
    private async Task GetMoviesSubscriptions(ApplicationState state, string emailValue, CancellationToken token)
    {
        try
        {
            var subscriptions = await _moviesSubscriberService.GetSubscriptions(emailValue, token);
            state.SetMoviesSubscriptions(subscriptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            state.SetMoviesSubscriptions(ImmutableList<string>.Empty);
        }
        catch (Exception ex)
        {
            state.ReportException(new AppException("Getting Movies Subscriptions", ex));
        }
    }
}