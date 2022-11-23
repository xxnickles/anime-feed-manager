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

namespace AnimeFeedManager.WebApp.Authentication
{
    class AppServiceAuthRemoteAuthenticationService<TAuthenticationState> : AuthenticationStateProvider, IRemoteAuthenticationService<TAuthenticationState> where TAuthenticationState : RemoteAuthenticationState
    {
        const string BrowserStorageType = "sessionStorage";
        const string StorageKeyPrefix = "Blazor.AppServiceAuth";
        readonly AppServiceAuthMemoryStorage _memoryStorage;

        public RemoteAuthenticationOptions<AppServiceAuthOptions> Options { get; }
        public HttpClient HttpClient { get; }
        public NavigationManager Navigation { get; }
        public IJSRuntime JsRuntime { get; }

        public AppServiceAuthRemoteAuthenticationService(
            IOptions<RemoteAuthenticationOptions<AppServiceAuthOptions>> options,
            NavigationManager navigationManager,
            IJSRuntime jsRuntime,
            AppServiceAuthMemoryStorage memoryStorage)
        {
            this.Options = options.Value;
            this.HttpClient = new HttpClient() { BaseAddress = new Uri(navigationManager.BaseUri) };
            this.Navigation = navigationManager;
            this.JsRuntime = jsRuntime;
            this._memoryStorage = memoryStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                if (this._memoryStorage.AuthenticationData == null)
                {
                    string authDataUrl = this.Options.ProviderOptions.AuthenticationDataUrl + "/.auth/me";
                    AuthenticationData data = await this.HttpClient.GetFromJsonAsync<AuthenticationData>(authDataUrl);
                    this._memoryStorage.SetAuthenticationData(data);
                }

                ClientPrincipal principal = this._memoryStorage.AuthenticationData.ClientPrincipal;

                if (principal == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal());
                }

                principal.UserRoles = principal.UserRoles.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

                var identity = new ClaimsIdentity(principal.IdentityProvider);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
                identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
                identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                this._memoryStorage.SetAuthenticationData(null);
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
            await this.JsRuntime.InvokeVoidAsync($"{BrowserStorageType}.setItem", $"{StorageKeyPrefix}.{stateId}", JsonSerializer.Serialize(context.State));
            this.Navigation.NavigateTo($"/.auth/login/{appServiceAuthContext.SelectedProvider}?post_login_redirect_uri={this.BuildRedirectUri(this.Options.AuthenticationPaths.LogInCallbackPath)}/{stateId}", forceLoad: true);

            return new RemoteAuthenticationResult<TAuthenticationState> { Status = RemoteAuthenticationStatus.Redirect };
        }

        public async Task<RemoteAuthenticationResult<TAuthenticationState>> CompleteSignInAsync(RemoteAuthenticationContext<TAuthenticationState> context)
        {
            string stateId = new Uri(context.Url).PathAndQuery.Split("?")[0].Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
            string serializedState = await this.JsRuntime.InvokeAsync<string>($"{BrowserStorageType}.getItem", $"{StorageKeyPrefix}.{stateId}");
            TAuthenticationState state = JsonSerializer.Deserialize<TAuthenticationState>(serializedState);
            return new RemoteAuthenticationResult<TAuthenticationState> { State = state, Status = RemoteAuthenticationStatus.Success };
        }

        public async Task<RemoteAuthenticationResult<TAuthenticationState>> CompleteSignOutAsync(RemoteAuthenticationContext<TAuthenticationState> context)
        {
            string[] sessionKeys = await this.JsRuntime.InvokeAsync<string[]>("eval", $"Object.keys({BrowserStorageType})");

            string stateKey = sessionKeys.FirstOrDefault(key => key.StartsWith(StorageKeyPrefix));

            if (stateKey != null)
            {
                await this.JsRuntime.InvokeAsync<string>($"{BrowserStorageType}.removeItem", stateKey);
            }

            return new RemoteAuthenticationResult<TAuthenticationState> { Status = RemoteAuthenticationStatus.Success };
        }

        public Task<RemoteAuthenticationResult<TAuthenticationState>> SignOutAsync(RemoteAuthenticationContext<TAuthenticationState> context)
        {
            this.Navigation.NavigateTo($"/.auth/logout?post_logout_redirect_uri={this.BuildRedirectUri(this.Options.AuthenticationPaths.LogOutCallbackPath)}", forceLoad: true);

            return Task.FromResult(new RemoteAuthenticationResult<TAuthenticationState> { Status = RemoteAuthenticationStatus.Redirect });
        }

        string BuildRedirectUri(string path)
        {
            return new Uri(new Uri(this.Navigation.BaseUri), path).ToString();
        }
    }
}
