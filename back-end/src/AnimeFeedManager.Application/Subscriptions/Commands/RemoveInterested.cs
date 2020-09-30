using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Commands
{
    public class RemoveInterested : Record<RemoveInterested>, IRequest<Either<DomainError, LanguageExt.Unit>>
    {
        public string Subscriber { get; }
        public string AnimeId { get; }

        public RemoveInterested(string subscriber, string animeId)
        {
            Subscriber = subscriber;
            AnimeId = animeId;
        }
    }
}