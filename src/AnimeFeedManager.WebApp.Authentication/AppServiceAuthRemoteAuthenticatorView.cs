// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace AnimeFeedManager.WebApp.Authentication;

public class
    AppServiceAuthRemoteAuthenticatorView : AppServiceAuthRemoteAuthenticatorViewCore<RemoteAuthenticationState>
{
    public AppServiceAuthRemoteAuthenticatorView() => AuthenticationState = new RemoteAuthenticationState();
}

public class
    AppServiceAuthRemoteAuthenticatorViewCore<TAuthenticationState> : RemoteAuthenticatorViewCore<
        TAuthenticationState> where TAuthenticationState : RemoteAuthenticationState
{
    string _message;

    [Parameter] public string SelectedOption { get; set; }

    [Inject] NavigationManager Navigation { get; set; }

    [Inject] IJSRuntime Js { get; set; }

    [Inject] IRemoteAuthenticationService<TAuthenticationState> AuthenticationService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        switch (Action)
        {
            case RemoteAuthenticationActions.LogIn:
                if (SelectedOption != null)
                {
                    await ProcessLogin(GetReturnUrl(state: null));
                }

                return;

            // Doing this because the SignOutManager intercepts the call otherwise and it'll fail
            // TODO: Investigate a custom SignOutManager
            case RemoteAuthenticationActions.LogOut:
                RemoteAuthenticationResult<TAuthenticationState> result =
                    await AuthenticationService.SignOutAsync(
                        new AppServiceAuthRemoteAuthenticationContext<TAuthenticationState>
                            {State = AuthenticationState});
                switch (result.Status)
                {
                    case RemoteAuthenticationStatus.Redirect:
                        break;
                    case RemoteAuthenticationStatus.Success:
                        await OnLogOutSucceeded.InvokeAsync(result.State);
                        await NavigateToReturnUrl(GetReturnUrl(AuthenticationState));
                        break;
                    case RemoteAuthenticationStatus.OperationCompleted:
                        break;
                    case RemoteAuthenticationStatus.Failure:
                        Navigation.NavigateTo(ApplicationPaths.LogOutFailedPath);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid authentication result status.");
                }

                break;

            default:
                await base.OnParametersSetAsync();
                break;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        switch (Action)
        {
            case RemoteAuthenticationActions.LogInFailed:
                builder.AddContent(0, LogInFailed(_message));
                break;
            case RemoteAuthenticationActions.LogOutFailed:
                builder.AddContent(0, LogOutFailed(_message));
                break;
            default:
                base.BuildRenderTree(builder);
                break;
        }
    }

    async Task ProcessLogin(string returnUrl)
    {
        AuthenticationState.ReturnUrl = returnUrl;
        RemoteAuthenticationResult<TAuthenticationState> result =
            await AuthenticationService.SignInAsync(
                new AppServiceAuthRemoteAuthenticationContext<TAuthenticationState>
                {
                    State = AuthenticationState,
                    SelectedProvider = SelectedOption
                });

        switch (result.Status)
        {
            case RemoteAuthenticationStatus.Redirect:
                break;
            case RemoteAuthenticationStatus.Success:
                await OnLogInSucceeded.InvokeAsync(result.State);
                await NavigateToReturnUrl(GetReturnUrl(result.State, returnUrl));
                break;
            case RemoteAuthenticationStatus.Failure:
                _message = result.ErrorMessage;
                Navigation.NavigateTo(ApplicationPaths.LogInFailedPath);
                break;
            case RemoteAuthenticationStatus.OperationCompleted:
                break;
            default:
                break;
        }
    }

    ValueTask NavigateToReturnUrl(string returnUrl)
    {
        return Js.InvokeVoidAsync("Blazor.navigateTo", returnUrl, false, true);
    }

    string GetReturnUrl(RemoteAuthenticationState state, string defaultReturnUrl = null)
    {
        if (state?.ReturnUrl != null)
        {
            return state.ReturnUrl;
        }

        string fromQuery = GetParameter(new Uri(Navigation.Uri).Query, "returnUrl");
        if (!string.IsNullOrWhiteSpace(fromQuery) && !fromQuery.StartsWith(Navigation.BaseUri))
        {
            // This is an extra check to prevent open redirects.
            throw new InvalidOperationException(
                "Invalid return url. The return url needs to have the same origin as the current page.");
        }

        return fromQuery ?? defaultReturnUrl ?? Navigation.BaseUri;
    }

    internal static string GetParameter(string queryString, string key)
    {
        if (string.IsNullOrEmpty(queryString) || queryString == "?")
        {
            return null;
        }

        int scanIndex = 0;
        if (queryString[0] == '?')
        {
            scanIndex = 1;
        }

        int textLength = queryString.Length;
        int equalIndex = queryString.IndexOf('=');
        if (equalIndex == -1)
        {
            equalIndex = textLength;
        }

        while (scanIndex < textLength)
        {
            int ampersandIndex = queryString.IndexOf('&', scanIndex);
            if (ampersandIndex == -1)
            {
                ampersandIndex = textLength;
            }

            if (equalIndex < ampersandIndex)
            {
                while (scanIndex != equalIndex && char.IsWhiteSpace(queryString[scanIndex]))
                {
                    ++scanIndex;
                }

                string name = queryString[scanIndex..equalIndex];
                string value = queryString.Substring(equalIndex + 1, ampersandIndex - equalIndex - 1);
                string processedName = Uri.UnescapeDataString(name.Replace('+', ' '));
                if (string.Equals(processedName, key, StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(value.Replace('+', ' '));
                }

                equalIndex = queryString.IndexOf('=', ampersandIndex);
                if (equalIndex == -1)
                {
                    equalIndex = textLength;
                }
            }
            else
            {
                if (ampersandIndex > scanIndex)
                {
                    string value = queryString[scanIndex..ampersandIndex];
                    if (string.Equals(value, key, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Empty;
                    }
                }
            }

            scanIndex = ampersandIndex + 1;
        }

        return null;
    }
}

public class
    AppServiceAuthRemoteAuthenticationContext<TAuthenticationState> : RemoteAuthenticationContext<
        TAuthenticationState> where TAuthenticationState : RemoteAuthenticationState
{
    public string SelectedProvider { get; set; }
}