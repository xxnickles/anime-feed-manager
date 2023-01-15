namespace AnimeFeedManager.Common.Notifications;

public readonly record struct NotificationType
{
    private const string TvValue = "TV";
    private const string TvTitlesValue = "TVTITLES";
    private const string OvaValue = "OVA";
    private const string MovieValue= "MOVIE";
    private const string AdminValue = "ADMIN";
    private const string ImagesValue = "IMAGES";
    private const string NoneValue = "NONE";
    
    public string Value { get; }

    private NotificationType(string value)
    {
        Value = value;
    }

    public static NotificationType Tv = new(TvValue);
    public static NotificationType TvTitles = new(TvTitlesValue);
    public static NotificationType Ova = new(OvaValue);
    public static NotificationType Movie = new(MovieValue);
    public static NotificationType Admin = new(AdminValue);
    public static NotificationType Images = new(ImagesValue);
    public static NotificationType None = new(NoneValue);
   
    
    public static NotificationType Parse(string value)
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
}




