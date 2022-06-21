using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Commands;

public sealed class MergeSubscription : Record<MergeSubscription>, IRequest<Either<DomainError, LanguageExt.Unit>>
{
    public string Subscriber { get; }
    public string AnimeId { get; }

    public MergeSubscription(string subscriber, string animeId)
    {
        Subscriber = subscriber;
        AnimeId = animeId;
    }
}