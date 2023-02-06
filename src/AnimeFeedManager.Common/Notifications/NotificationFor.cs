namespace AnimeFeedManager.Common.Notifications;

public enum NotificationForValue
{
    None,
    Tv,
    TvTitles,
    Ova,
    Movie,
    Admin,
    Image
}

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
    
    public NotificationForValue Type { get; }

    private NotificationFor(string value, NotificationForValue type)
    {
        Value = value;
        Type = type;
    }

    public static NotificationFor Tv = new(TvValue, NotificationForValue.Tv);
    public static NotificationFor TvTitles = new(TvTitlesValue, NotificationForValue.TvTitles);
    public static NotificationFor Ova = new(OvaValue, NotificationForValue.Ova);
    public static NotificationFor Movie = new(MovieValue, NotificationForValue.Movie);
    public static NotificationFor Admin = new(AdminValue, NotificationForValue.Admin);
    public static NotificationFor Images = new(ImagesValue, NotificationForValue.Image);
    public static NotificationFor None = new(NoneValue, NotificationForValue.None);
   
    
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




