using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Services.Collectors.AniDb;

internal static class Utils
{
    internal static (string Season, ushort Year) UnwrapForAniDb(this SeasonInformation seasonInformation)
    {
       return (seasonInformation.Season.ToAlternativeString(),
               seasonInformation.Year.Value.UnpackOption((ushort)0));
    }
}