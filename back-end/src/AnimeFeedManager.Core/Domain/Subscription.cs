using AnimeFeedManager.Core.ConstrainedTypes;
using LanguageExt;

namespace AnimeFeedManager.Core.Domain;

public class Subscription : Record<Subscription>
{
    public Email Subscriber { get; }
    public NonEmptyString AnimeId { get; }

    public Subscription(Email subscriber, NonEmptyString animeId)
    {
        Subscriber = subscriber;
        AnimeId = animeId;
    }
}