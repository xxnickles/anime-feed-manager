namespace AnimeFeedManager.Features.Common.Dto;

public static class DtoMappers
{
    public static SimpleSeasonInfo Map(SeasonInformation seasonInformation, bool isLatest)
    {
        return new SimpleSeasonInfo(seasonInformation.Season.Value, seasonInformation.Year.Value.UnpackOption<ushort>(0), isLatest);
    }
}