namespace AnimeFeedManager.Features.ProcessState.Types;


public enum Target
{
    None,
    Tv,
    Ova,
    Movie,
    Admin,
    Image
}


public readonly record struct StateProcessTarget : IEquatable<string>
{
    private const string TvValue = "TV";
    private const string OvaValue = "OVA";
    private const string MovieValue= "MOVIE";
    private const string AdminValue = "ADMIN";
    private const string ImagesValue = "IMAGES";
    private const string NoneValue = "NONE";
    
    public string Value { get; }
    
    public Target Type { get; }

    private StateProcessTarget(string value, Target type)
    {
        Value = value;
        Type = type;
    }

    public static StateProcessTarget Tv = new(TvValue, Target.Tv);
    public static StateProcessTarget Ova = new(OvaValue, Target.Ova);
    public static StateProcessTarget Movie = new(MovieValue, Target.Movie);
    public static StateProcessTarget Admin = new(AdminValue, Target.Admin);
    public static StateProcessTarget Images = new(ImagesValue, Target.Image);
    public static StateProcessTarget None = new(NoneValue, Target.None);
   
    public static implicit operator string(StateProcessTarget target) => target.Value;
    public static StateProcessTarget Parse(string value)
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