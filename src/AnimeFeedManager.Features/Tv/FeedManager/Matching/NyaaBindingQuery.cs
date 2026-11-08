using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tv.FeedManager.Matching;

public static class NyaaBindingQuery
{
    // Group and title are separate phrases: some groups write "[Group] - Title"; a trusted binding filters server-side to stretch the 75-item window.
    public static NyaaQuery For(NyaaBindingStorage binding)
    {
        var query = NyaaQuery.Create().Search($"\"{binding.Group}\" \"{binding.ReleaseTitle}\"");
        return binding.Trusted ? query.TrustedOnly() : query;
    }
}
