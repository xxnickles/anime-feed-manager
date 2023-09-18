namespace AnimeFeedManager.Features.Common.Domain.Notifications;

public enum Target
{
    None,
    Tv,
    Ova,
    Movie,
    Admin,
    Image
}

public readonly record struct NotificationTarget : IEquatable<string>
{
    private const string TvValue = "TV";
    private const string OvaValue = "OVA";
    private const string MovieValue= "MOVIE";
    private const string AdminValue = "ADMIN";
    private const string ImagesValue = "IMAGES";
    private const string NoneValue = "NONE";
    
    public string Value { get; }
    
    public Target Type { get; }

    private NotificationTarget(string value, Target type)
    {
        Value = value;
        Type = type;
    }

    public static NotificationTarget Tv = new(TvValue, Target.Tv);
    public static NotificationTarget Ova = new(OvaValue, Target.Ova);
    public static NotificationTarget Movie = new(MovieValue, Target.Movie);
    public static NotificationTarget Admin = new(AdminValue, Target.Admin);
    public static NotificationTarget Images = new(ImagesValue, Target.Image);
    public static NotificationTarget None = new(NoneValue, Target.None);
   
    
    public static NotificationTarget Parse(string value)
    {
        return value switch
        {
            TvValue => Tv,
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