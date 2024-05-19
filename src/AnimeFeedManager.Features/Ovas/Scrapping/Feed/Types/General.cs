using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

public enum OvaFeedScrapResult
{
    NotFound,
    FoundAndUpdated
}

