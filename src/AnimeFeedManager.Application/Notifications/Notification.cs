using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AnimeFeedManager.Core.Domain;

namespace AnimeFeedManager.Application.Notifications;

public sealed record SubscribedFeed(string Title, IImmutableList<TorrentLink> Links, string EpisodeInfo, DateTime PublicationDate);

public sealed record Notification(string Subscriber, IEnumerable<SubscribedFeed> Feeds);