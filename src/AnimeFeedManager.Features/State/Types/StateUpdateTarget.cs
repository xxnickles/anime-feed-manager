namespace AnimeFeedManager.Features.State.Types;

public enum Target
{
    None,
    Tv,
    Ova,
    Movie,
    Admin,
    Image
}

public readonly record struct StateUpdateTarget : IEquatable<string>
{
    private const string TvValue = "TV";
    private const string OvaValue = "OVA";
    private const string MovieValue= "MOVIE";
    private const string AdminValue = "ADMIN";
    private const string ImagesValue = "IMAGES";
    private const string NoneValue = "NONE";
    
    public string Value { get; }
    
    public Target Type { get; }

    private StateUpdateTarget(string value, Target type)
    {
        Value = value;
        Type = type;
    }

    public static StateUpdateTarget Tv = new(TvValue, Target.Tv);
    public static StateUpdateTarget Ova = new(OvaValue, Target.Ova);
    public static StateUpdateTarget Movie = new(MovieValue, Target.Movie);
    public static StateUpdateTarget Admin = new(AdminValue, Target.Admin);
    public static StateUpdateTarget Images = new(ImagesValue, Target.Image);
    public static StateUpdateTarget None = new(NoneValue, Target.None);
   
    
    public static StateUpdateTarget Parse(string value)
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




