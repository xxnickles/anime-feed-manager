using System.Collections.Immutable;
using System.Net;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.WebApp.Services;
using AnimeFeedManager.WebApp.Services.Ovas;
using AnimeFeedManager.WebApp.Services.Tv;
using MudBlazor;

namespace AnimeFeedManager.WebApp.State;

public record struct UserInformation(
    string UserId,
    string UserName,
    bool IsAdmin
);

public sealed class UserSideEffects
{
    private record UserEmail(string Email);

    private record InvalidEmail(string UserId) : UserEmail(string.Empty);

    private record LocalEmail(string UserId, string Email) : UserEmail(Email);

    private record UserNameEmail(string UserId, string Email) : UserEmail(Email);


    private readonly ITvSubscriberService _tvSubscriberService;
    private readonly IOvasSubscriberService _ovasSubscriberService;
    private readonly IUserService _userService;

    public UserSideEffects(
        ITvSubscriberService tvSubscriberService,
        IOvasSubscriberService ovasSubscriberService,
        IUserService userService)
    {
        _tvSubscriberService = tvSubscriberService;
        _ovasSubscriberService = ovasSubscriberService;
        _userService = userService;
    }


    public static void CompleteDefaultProfile(ApplicationState state, User user)
    {
        state.SetUser(user);
        state.SetSubscriptions(ImmutableList<string>.Empty);
        state.SetInterested(ImmutableList<string>.Empty);
        state.SetOvasSubscriptions(ImmutableList<string>.Empty);
    }

    public async Task CompleteUserProfile(
        ApplicationState state,
        UserInformation userInformation,
        Func<Task<string>> localEmailProvider,
        CancellationToken token)
    {
        var email = await GetUserEmail(
            state,
            userInformation.UserId,
            userInformation.UserName,
            localEmailProvider,
            token);

        var user = await GetUser(state, email, userInformation.IsAdmin, token);
        state.SetUser(user);

        var task = user switch
        {
            AdminUser au => CompleteProfile(state, au.Email, token),
            ApplicationUser ap => CompleteProfile(state, ap.Email, token),
            _ => Task.CompletedTask
        };

        await task;
    }

    private async Task<UserEmail> GetUserEmail(
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
            var userEmail = await _userService.GetEmail(userId, token);
            if (Email.IsEmail(userEmail ?? string.Empty))
            {
                return new UserEmail(userEmail ?? string.Empty);
            }
            else
            {
                if (Email.IsEmail(userName))
                {
                    return new UserNameEmail(userId, userName);
                }
            }

            return await GetEmailFromLocal(userId, localEmailProvider);
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.NotFound)
        {
            return await GetEmailFromLocal(userId, localEmailProvider);
        }
        catch (Exception ex)
        {
            state.ReportException(new AppException("User Fetching", ex));
            return new InvalidEmail(userId);
        }
        finally
        {
            state.RemoveLoadingItem(key);
        }
    }

    private Task<User> GetUser(ApplicationState state, UserEmail userEmail, bool isAdmin, CancellationToken token)
    {
        return userEmail switch
        {
            LocalEmail le => TryPersistUser(state, le.UserId, le.Email, isAdmin, token),
            UserNameEmail ue => TryPersistUser(state, ue.UserId, ue.Email, isAdmin, token),
            InvalidEmail ie => Task.FromResult<User>(new AuthenticatedUser(ie.UserId)),
            UserEmail => isAdmin
                ? Task.FromResult<User>(new AdminUser(userEmail.Email))
                : Task.FromResult<User>(new ApplicationUser(userEmail.Email)),
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
            await _userService.MergeUser(new UserDto(userId, email), token);
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

    private static async Task<UserEmail> GetEmailFromLocal(string userId, Func<Task<string>> localEmailProvider)
    {
        var emailValue = await localEmailProvider();
        return Email.IsEmail(emailValue) ? new LocalEmail(userId, emailValue) : new InvalidEmail(userId);
    }

    private async Task CompleteProfile(ApplicationState state, string emailValue, CancellationToken token)
    {
        await GetTvSubscriptions(state, emailValue, token);
        await GetTvInterested(state, emailValue, token);
        await GetOvasSubscriptions(state, emailValue, token);
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
            state.ReportException(new AppException("Getting Subscriptions", ex));
        }
    }
}