namespace AnimeFeedManager.Features.Infrastructure.Messaging
{
    public readonly struct Box
    {
        public readonly struct Available
        {
            public const string SeasonProcessNotificationsBox = "season-process-notifications";
            public const string TitleUpdatesNotificationsBox = "title-updtates-notifications";
            public const string ImageUpdateNotificationsBox = "image-update-notifications";
            public const string ImageProcessBox = "image-process";
            public const string LibraryScrapEventsBox = "library-scrap-events";
            public const string UserAutoSubscriptionBox = "user-auto-subscription";
            public const string AutoSubscriptionsProcessBox = "auto-subscriptions-process";
            public const string TvNotificationsBox = "tv-notifications";
            public const string SeasonTitlesProcessBox = "season-titles-process";
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
    
        public static Box SeasonProcessNotifications => new (Available.SeasonProcessNotificationsBox);
        public static Box TitleUpdatesNotifications => new (Available.TitleUpdatesNotificationsBox);
        public static Box ImageUpdateNotifications => new (Available.ImageUpdateNotificationsBox);
        public static Box ImageProcess => new (Available.ImageProcessBox);
        public static Box LibraryScrapEvents => new(Available.LibraryScrapEventsBox);
        public static Box UserAutoSubscription => new(Available.UserAutoSubscriptionBox);
        public static Box AutoSubscriptionsProcess => new(Available.AutoSubscriptionsProcessBox);
        public static Box TvNotifications => new(Available.TvNotificationsBox);
    
        public static Box SeasonTitlesProcess=> new(Available.SeasonTitlesProcessBox);
    }
}