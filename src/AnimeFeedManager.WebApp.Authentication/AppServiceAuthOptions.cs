// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace AnimeFeedManager.WebApp.Authentication;

public class AppServiceAuthOptions
{
    public IList<ExternalProvider> Providers { get; set; } = new List<ExternalProvider> {
        new("github", "GitHub"),
        new("twitter", "Twitter"),
        new("aad", "Azure Active Directory")
    };
    public string AuthenticationDataUrl { get; set; } = "";
}

public class ExternalProvider(string id, string name)
{
    public string Id { get; set; } = id;
    public string DisplayName { get; set; } = name;
}