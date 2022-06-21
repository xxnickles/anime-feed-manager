using AnimeFeedManager.Core.ConstrainedTypes;
using LanguageExt;

namespace AnimeFeedManager.Core.Domain;

public class InterestedSeries : Record<InterestedSeries>
{
    public Email Subscriber { get; }
    public NonEmptyString AnimeId { get; }

    public InterestedSeries(Email subscriber, NonEmptyString animeId)
    {
        Subscriber = subscriber;
        AnimeId = animeId;
    }
}