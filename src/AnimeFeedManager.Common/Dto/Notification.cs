﻿using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Common.Dto;

public record SubscribedFeed(string Title, TorrentLink[] Links, string EpisodeInfo, DateTime PublicationDate);

public record SubscriberTvNotification(string Subscriber, string SubscriberId, SubscribedFeed[] Feeds)
    : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "tv-notifications";
}


[JsonSerializable(typeof(SubscribedFeed))]
[JsonSerializable(typeof(SubscriberTvNotification))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SubscriberNotificationContext : JsonSerializerContext
{
}

public sealed record NotifiedTitle(string Subscriber, string Title);

public readonly record struct UiNotification(string Type, DateTimeOffset TimeOffset, string Payload);

public record UiNotifications(
    UiNotification[] TvNotifications,
    UiNotification[] OvasNotifications,
    UiNotification[] MoviesNotifications,
    UiNotification[] ImagesNotifications,
    UiNotification[] AdminNotifications);

public record EmptyUiNotifications() : UiNotifications(
    [],
    [],
    [],
    [],
    []);