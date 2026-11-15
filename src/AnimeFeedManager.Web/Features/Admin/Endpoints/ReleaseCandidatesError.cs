using AnimeFeedManager.Features.Scrapping;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

/// <summary>A feed series read followed by a nyaa release search.</summary>
[Widens<StorageRequestError>]
internal partial union ReleaseCandidatesError(ValidationError, StorageError, ScrapeError);

internal static class ReleaseCandidatesErrorContent
{
    extension(ReleaseCandidatesError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError validation => validation.ToContent(),
            StorageError storage => storage.ToContent(),
            ScrapeError scrape => scrape.ToContent()
        };
    }
}
