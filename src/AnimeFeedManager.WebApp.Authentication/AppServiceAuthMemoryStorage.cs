// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AnimeFeedManager.WebApp.Authentication.Models;

namespace AnimeFeedManager.WebApp.Authentication;

// A simple in-memory storage model for caching auth data
internal class AppServiceAuthMemoryStorage
{
    public AuthenticationData AuthenticationData { get; private set; }

    public void SetAuthenticationData(AuthenticationData data)
    {
        AuthenticationData = data;
    }
}