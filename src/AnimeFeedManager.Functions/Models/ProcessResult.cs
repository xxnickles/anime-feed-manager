namespace AnimeFeedManager.Functions.Models;

internal struct ProcessResult
{
    internal const string Ok = "Ok";
    internal const string Failure = "Failure";
    internal const string NoChanges = "NoChanges";
}

public enum LibraryUpdateType
{
    Full,
    Titles
}

public record struct LibraryUpdate(LibraryUpdateType Type);

public record struct OvasUpdate(string Type = ProcessResult.Ok);

