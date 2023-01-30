namespace AnimeFeedManager.Common.Notifications;

public readonly record struct NotificationFor : IEquatable<string>
{
    private const string TvValue = "TV";
    private const string TvTitlesValue = "TVTITLES";
    private const string OvaValue = "OVA";
    private const string MovieValue= "MOVIE";
    private const string AdminValue = "ADMIN";
    private const string ImagesValue = "IMAGES";
    private const string NoneValue = "NONE";
    
    public string Value { get; }

    private NotificationFor(string value)
    {
        Value = value;
    }

    public static NotificationFor Tv = new(TvValue);
    public static NotificationFor TvTitles = new(TvTitlesValue);
    public static NotificationFor Ova = new(OvaValue);
    public static NotificationFor Movie = new(MovieValue);
    public static NotificationFor Admin = new(AdminValue);
    public static NotificationFor Images = new(ImagesValue);
    public static NotificationFor None = new(NoneValue);
   
    
    public static NotificationFor Parse(string value)
    {
        return value switch
        {
            TvValue => Tv,
            TvTitlesValue => TvTitles,
            MovieValue => Movie,
            OvaValue => Ova,
            AdminValue => Admin,
            ImagesValue => Images,
            _ => None
        };
    }
    
    public bool Equals(string? other)
    {
        return other == Value;
    }
   
}




