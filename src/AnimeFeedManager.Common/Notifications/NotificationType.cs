namespace AnimeFeedManager.Common.Notifications;

public enum NotificationTypeValue {
    None,
    Feed,
    Update,
    Error
}

public readonly record struct NotificationType
{
    private const string FeedValue = "FEED";
    private const string UpdateValue = "UPDATE";
    private const string ErrorValue = "ERROR";
    private const string NoneValue = "NONE";
    
    public string Value { get; }
    public NotificationTypeValue Type {get;}

    private NotificationType(string value, NotificationTypeValue type)
    {
        Value = value;
        Type = type;
    }

    public static NotificationType Feed = new(FeedValue, NotificationTypeValue.Feed);
    public static NotificationType Update = new(UpdateValue, NotificationTypeValue.Update);
    public static NotificationType Error = new(ErrorValue, NotificationTypeValue.Error);
    public static NotificationType None = new(NoneValue, NotificationTypeValue.None);

    
    public static NotificationType Parse(string value)
    {
        return value switch
        {
            FeedValue => Feed,
            UpdateValue => Update,
            ErrorValue => Error,
            _ => None
        };
    }

}




