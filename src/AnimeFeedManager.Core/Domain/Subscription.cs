using System.Collections.Immutable;
using AnimeFeedManager.Core.ConstrainedTypes;
using LanguageExt;

namespace AnimeFeedManager.Core.Domain
{
    public class Subscription : Record<Subscription>
    {
        public Email Subscriber { get; }
        public ImmutableList<NonEmptyString> AnimeIds { get; }

        public Subscription(Email subscriber, ImmutableList<NonEmptyString> animeIds)
        {
            Subscriber = subscriber;
            AnimeIds = animeIds;
        }
    }
}
