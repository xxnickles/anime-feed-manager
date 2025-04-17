using AnimeFeedManager.Old.Common.Domain.Events;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Series.Types;

public record CompleteAlternativeTitle(string Id, string Partition)
    : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "complete-alternative-title";
}