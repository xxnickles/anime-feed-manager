using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Functions.Models;

internal struct ProcessResult
{
    internal const string Ok = "Ok";
    internal const string Failure = "Failure";
    internal const string NoChanges = "NoChanges";
}

public enum TvUpdateType
{
    Full,
    Titles
}

public readonly record struct LibraryUpdate(TvUpdateType Type);

public enum ShortSeriesUpdateType
{
    Latest,
    Season
}

public readonly record struct OvasUpdate(ShortSeriesUpdateType Type, SeasonInfoDto? SeasonInformation);
public readonly record struct MoviesUpdate(ShortSeriesUpdateType Type, SeasonInfoDto? SeasonInformation);

