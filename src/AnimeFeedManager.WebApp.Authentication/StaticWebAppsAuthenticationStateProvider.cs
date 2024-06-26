// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Text.Json;
using AnimeFeedManager.WebApp.Authentication.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;

namespace AnimeFeedManager.WebApp.Authentication;

public class StaticWebAppsAuthenticationStateProvider(IConfiguration config, IWebAssemblyHostEnvironment environment)
    : AuthenticationStateProvider
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(environment.BaseAddress) };

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string authDataUrl = config.GetValue("StaticWebAppsAuthentication:AuthenticationDataUrl", "/.auth/me");
            string json = await _http.GetStringAsync(authDataUrl);

            ClaimsPrincipal user = ParseClaims(json);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }

    public static ClaimsPrincipal ParseClaims(string json)
    {
        AuthenticationData data = JsonSerializer.Deserialize<AuthenticationData>(json);

        ClientPrincipal principal = data.ClientPrincipal;
        principal.UserRoles = principal.UserRoles.Except(new[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

        if (!principal.UserRoles.Any())
        {
            return new ClaimsPrincipal();
        }

        var identity = new ClaimsIdentity(principal.IdentityProvider);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
        identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
        identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));
        return new ClaimsPrincipal(identity);
    }
}