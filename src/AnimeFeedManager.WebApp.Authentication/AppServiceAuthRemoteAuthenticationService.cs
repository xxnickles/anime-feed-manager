// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using AnimeFeedManager.WebApp.Authentication.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace AnimeFeedManager.WebApp.Authentication;

internal class AppServiceAuthRemoteAuthenticationService<TAuthenticationState>(
    IOptions<RemoteAuthenticationOptions<AppServiceAuthOptions>> options,
    NavigationManager navigationManager,
    IJSRuntime jsRuntime,
    AppServiceAuthMemoryStorage memoryStorage)
    : AuthenticationStateProvider, IRemoteAuthenticationService<TAuthenticationState>
    where TAuthenticationState : RemoteAuthenticationState
{
    private const string BrowserStorageType = "sessionStorage";
    private const string StorageKeyPrefix = "Blazor.AppServiceAuth";

    public RemoteAuthenticationOptions<AppServiceAuthOptions> Options { get; } = options.Value;
    public HttpClient HttpClient { get; } = new() { BaseAddress = new Uri(navigationManager.BaseUri) };
    public NavigationManager Navigation { get; } = navigationManager;
    public IJSRuntime JsRuntime { get; } = jsRuntime;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            if (memoryStorage.AuthenticationData == null)
            {
                string authDataUrl = Options.ProviderOptions.AuthenticationDataUrl + "/.auth/me";
                AuthenticationData data = await HttpClient.GetFromJsonAsync<AuthenticationData>(authDataUrl);
                memoryStorage.SetAuthenticationData(data);
            }

            ClientPrincipal principal = memoryStorage.AuthenticationData.ClientPrincipal;

            if (principal == null)
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }

            principal.UserRoles = principal.UserRoles.Except(new[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

            var identity = new ClaimsIdentity(principal.IdentityProvider);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            memoryStorage.SetAuthenticationData(null);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task<RemoteAuthenticationResult<TAuthenticationState>> SignInAsync(RemoteAuthenticationContext<TAuthenticationState> context)
    {
        if (!(context is AppServiceAuthRemoteAuthenticationContext<TAuthenticationState> appServiceAuthContext))
        {
            throw new InvalidOperationException("Not an AppServiceAuthContext");
        }

        string stateId = Guid.NewGuid().ToString();
        await JsRuntime.InvokeVoidAsync($"{BrowserStorageType}.setItem", $"{StorageKeyPrefix}.{stateId}", JsonSerializer.Serialize(context.State));
        Navigation.NavigateTo($"/.auth/login/{appServiceAuthContext.SelectedProvider}?post_login_redirect_uri={BuildRedirectUri(Options.AuthenticationPaths.LogInCallbackPath)}/{stateId}", forceLoad: true);

        return new RemoteAuthenticationResult<TAuthenticationState> { Status = RemoteAuthenticationStatus.Redirect };
    }

    public async Task<RemoteAuthenticationResult<TAuthenticationState>> CompleteSignInAsync(RemoteAuthenticationContext<TAuthenticationState> context)
    {
        string stateId = new Uri(context.Url).PathAndQuery.Split("?")[0].Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
        string serializedState = await JsRuntime.InvokeAsync<string>($"{BrowserStorageType}.getItem", $"{StorageKeyPrefix}.{stateId}");
        TAuthenticationState state = JsonSerializer.Deserialize<TAuthenticationState>(serializedState);
        return new RemoteAuthenticationResult<TAuthenticationState> { State = state, Status = RemoteAuthenticationStatus.Success };
    }

    public async Task<RemoteAuthenticationResult<TAuthenticationState>> CompleteSignOutAsync(RemoteAuthenticationContext<TAuthenticationState> context)
    {
        string[] sessionKeys = await JsRuntime.InvokeAsync<string[]>("eval", $"Object.keys({BrowserStorageType})");

        string stateKey = sessionKeys.FirstOrDefault(key => key.StartsWith(StorageKeyPrefix));

        if (stateKey != null)
        {
            await JsRuntime.InvokeAsync<string>($"{BrowserStorageType}.removeItem", stateKey);
        }

        return new RemoteAuthenticationResult<TAuthenticationState> { Status = RemoteAuthenticationStatus.Success };
    }

    public Task<RemoteAuthenticationResult<TAuthenticationState>> SignOutAsync(RemoteAuthenticationContext<TAuthenticationState> context)
    {
        Navigation.NavigateTo($"/.auth/logout?post_logout_redirect_uri={BuildRedirectUri(Options.AuthenticationPaths.LogOutCallbackPath)}", forceLoad: true);

        return Task.FromResult(new RemoteAuthenticationResult<TAuthenticationState> { Status = RemoteAuthenticationStatus.Redirect });
    }

    private string BuildRedirectUri(string path)
    {
        return new Uri(new Uri(Navigation.BaseUri), path).ToString();
    }
}