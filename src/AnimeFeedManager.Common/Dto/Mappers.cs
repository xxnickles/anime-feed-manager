using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Common.Dto;

public static class Mappers
{
    public static SeasonInfoDto Map(this SeasonInformation @this)
    {
        return new SeasonInfoDto(@this.Season.Value, @this.Year.Value.UnpackOption<ushort>(0));
    }
}