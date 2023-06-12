using AnimeFeedManager.Features.Common.Types;
using AnimeFeedManager.Features.Common.Utils;

namespace AnimeFeedManager.Features.Common.Dto;

public static class Mappers
{
    public static SeasonInfoDto Map(this SeasonInformation @this)
    {
        return new SeasonInfoDto(@this.Season.Value, @this.Year.Value.UnpackOption<ushort>(0));
    }
}