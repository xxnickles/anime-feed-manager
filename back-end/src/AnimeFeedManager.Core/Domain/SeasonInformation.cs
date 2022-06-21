using AnimeFeedManager.Core.ConstrainedTypes;
using LanguageExt;

namespace AnimeFeedManager.Core.Domain;

public class SeasonInformation: Record<SeasonInformation>
{
    public readonly Season Season;
    public readonly Year Year;

    public SeasonInformation(Season season, Year year)
    {
        Season = season;
        Year = year;
    }
}