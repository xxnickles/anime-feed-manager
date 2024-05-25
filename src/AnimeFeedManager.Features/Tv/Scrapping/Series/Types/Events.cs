using AnimeFeedManager.Common.Domain.Events;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.Types;

public record CompleteAlternativeTitle(string Id, string Partition)
    : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "complete-alternative-title";
}