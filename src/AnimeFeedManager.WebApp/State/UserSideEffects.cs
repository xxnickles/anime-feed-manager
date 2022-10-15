using System.Collections.Immutable;
using System.Net;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.WebApp.Services;
using MudBlazor;

namespace AnimeFeedManager.WebApp.State;

public sealed class UserSideEffects
{
    private readonly ApplicationState _state;
    private readonly ITvSubscriberService _tvSubscriberService;
    private readonly IUserService _userService;

    public UserSideEffects(
        ApplicationState state,
        ITvSubscriberService tvSubscriberService,
        IUserService userService)
    {
        _state = state;
        _tvSubscriberService = tvSubscriberService;
        _userService = userService;
    }

    public Task CompleteDefaultProfile(User user)
    {
        _state.SetUser(user);
        _state.SetSubscriptions(ImmutableList<string>.Empty);
        _state.SetInterested(ImmutableList<string>.Empty);

        return Task.CompletedTask;
    }

    public async Task<(bool successful, Email email)> TryGetUserEmail(string userId, CancellationToken token)
    {
        var key = $"email_{userId}";
        _state.AddLoadingItem(key, "Loading user Email");
        try
        {
            var userEmail = await _userService.GetEmail(userId, token);
            return (true, Email.FromString(userEmail ?? string.Empty));
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.NotFound)
        {
            return (true, Email.FromString(string.Empty));
        }
        catch (Exception ex)
        {
            _state.ReportException(new AppException("User Fetching", ex));
            return (false, Email.FromString(string.Empty));
        }
        finally
        {
            _state.RemoveLoadingItem(key);
        }
    }
    
    public async Task SaveUser(string userId, string email, CancellationToken token)
    {
        var key = $"save_{userId}";
        _state.AddLoadingItem(key, "Storing User");
        try
        {
            await _userService.MergeUser(new UserDto(userId, email), token);
            _state.ReportNotification(new AppNotification("Email has been stored", Severity.Info));
            _state.RemoveLoadingItem(key);
        }
        catch (Exception ex)
        {
            _state.ReportException(new AppException("User Fetching", ex));
            _state.RemoveLoadingItem(key);
        }
    }

    public async Task CompleteProfile(string emailValue, bool isAdmin, CancellationToken token)
    {
        try
        {
            var subscriptions = await _tvSubscriberService.GetSubscriptions(emailValue, token);
            _state.SetSubscriptions(subscriptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _state.SetSubscriptions(ImmutableList<string>.Empty);
        }
        catch (Exception ex)
        {
            _state.ReportException(new AppException("Getting Subscriptions", ex));
        }

        try
        {
            var interested = await _tvSubscriberService.GetInterested(emailValue, token);
            _state.SetInterested(interested);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _state.SetSubscriptions(ImmutableList<string>.Empty);
        }
        catch (Exception ex)
        {
            _state.ReportException(new AppException("Getting Interested Series", ex));
        }

        _state.SetUser(isAdmin ? new AdminUser(emailValue) : new ApplicationUser(emailValue));
        _state.ReportNotification(new AppNotification("Profile has been completed", Severity.Info));
    }
}