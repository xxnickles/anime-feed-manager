namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Discriminated union describing which season a library import should fetch.
/// <see cref="CurrentSeason"/> resolves to whatever Jikan reports as the current
/// season at run time; <see cref="SpecificSeason"/> pins the import to a known
/// <see cref="Shared.Types.SeriesSeason"/>.
/// </summary>
public abstract record ImportTarget
{
    public sealed record CurrentSeason : ImportTarget;

    public sealed record SpecificSeason(SeriesSeason SeriesSeason) : ImportTarget;

    public static ImportTarget Now() => new CurrentSeason();
    public static ImportTarget For(SeriesSeason seriesSeason) => new SpecificSeason(seriesSeason);
}
