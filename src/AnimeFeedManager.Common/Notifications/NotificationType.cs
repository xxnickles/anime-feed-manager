namespace AnimeFeedManager.Common.Notifications;

public readonly record struct NotificationType
{
    private const string FeedValue = "FEED";
    private const string UpdateValue = "UPDATE";
    private const string ErrorValue = "ERROR";
    private const string NoneValue = "NONE";
    
    public string Value { get; }

    private NotificationType(string value)
    {
        Value = value;
    }

    public static NotificationType Feed = new(FeedValue);
    public static NotificationType Update = new(UpdateValue);
    public static NotificationType Error = new(ErrorValue);
    public static NotificationType None = new(NoneValue);
   
    
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
    
    public bool Equals(string? other)
    {
        return other == Value;
    }
}




