namespace AnimeFeedManager.Features.Infrastructure.Messaging;

public readonly struct Box
{
    public readonly struct Available
    {
        public const string SeasonProcessNotificationsBox = "season-process-notifications";
        public const string TitleUpdatesNotificationsBox = "title-update-notifications";
        public const string ImageUpdateNotificationsBox = "image-update-notifications";
        public const string ImageProcessBox = "image-process";
        public const string LibraryScrapEventsBox = "library-scrap-events";
        public const string TvTitlesScrapEventsBox = "tv-titles-scrap-events";
        public const string UserAutoSubscriptionBox = "user-auto-subscription";
        public const string AutoSubscriptionsProcessBox = "auto-subscriptions-process";
        public const string TvNotificationsBox = "tv-notifications";
        public const string SeasonTitlesProcessBox = "season-titles-process";
        public const string LatestSeasonsBox = "latest-seasons-process";
        public const string SubscriptionsRemovalBox = "subscriptions-removal";
        public const string SubscriptionsCopyBox = "subscriptions-copy";
        public const string ImageToScrapBox = "image-to-scrap";
        public const string AddSeasonBox = "add-seasson";
        public const string SeriesCompleterBox = "series-completer";
        public const string AutomatedSubscriptionBox = "automated-subscription";
        public const string AlternativeTitleUpdateBox = "alternative-title-update-box";
    }

    private readonly string _boxValue;

    private Box(string boxValue)
    {
        _boxValue = boxValue;
    }

    public override string ToString()
    {
        return _boxValue;
    }

    public static implicit operator string(Box box) => box._boxValue;

    public static Box SeasonProcessNotifications => new(Available.SeasonProcessNotificationsBox);
    public static Box TitleUpdatesNotifications => new(Available.TitleUpdatesNotificationsBox);
    public static Box ImageUpdateNotifications => new(Available.ImageUpdateNotificationsBox);
    public static Box ImageProcess => new(Available.ImageProcessBox);
    public static Box LibraryScrapEvents => new(Available.LibraryScrapEventsBox);
    public static Box TvTitlesScrapEvents => new(Available.TvTitlesScrapEventsBox);
    public static Box UserAutoSubscription => new(Available.UserAutoSubscriptionBox);
    public static Box AutoSubscriptionsProcess => new(Available.AutoSubscriptionsProcessBox);
    public static Box TvNotifications => new(Available.TvNotificationsBox);
    public static Box SeasonTitlesProcess => new(Available.SeasonTitlesProcessBox);
    public static Box LatestSeason => new(Available.LatestSeasonsBox);
    public static Box SubscriptionsRemoval => new(Available.SubscriptionsRemovalBox);
    public static Box SubscriptionsCopy => new(Available.SubscriptionsCopyBox);
    public static Box ImageToScrap => new(Available.ImageToScrapBox);
    public static Box AddSeason => new(Available.AddSeasonBox);
    public static Box SeriesCompleter => new(Available.SeriesCompleterBox);
    public static Box AutomatedSubscription => new(Available.AutomatedSubscriptionBox);
    
    public static Box AlternativeTitleUpdate => new(Available.AlternativeTitleUpdateBox);
}
