namespace AnimeFeedManager.Features.Common.Dto
{
    public static class DtoMappers
    {
        public static SeasonInfoDto Map(SeasonInformation seasonInformation)
        {
            return new SeasonInfoDto(seasonInformation.Season.Value, seasonInformation.Year.Value.UnpackOption<ushort>(0));
        }
    }
}