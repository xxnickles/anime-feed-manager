﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace AnimeFeedManager.WebApp.Authentication
{
    public class AppServiceAuthOptions
    {
        public IList<ExternalProvider> Providers { get; set; } = new List<ExternalProvider> {
            new ExternalProvider("github", "GitHub"),
            new ExternalProvider("twitter", "Twitter"),
            new ExternalProvider("aad", "Azure Active Directory")
        };
        public string AuthenticationDataUrl { get; set; } = "";
    }

    public class ExternalProvider
    {
        public ExternalProvider(string id, string name)
        {
            this.Id = id;
            this.DisplayName = name;
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
}