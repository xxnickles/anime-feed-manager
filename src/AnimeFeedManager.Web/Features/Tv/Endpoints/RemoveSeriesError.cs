namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal union RemoveSeriesError(ValidationError, StorageError);

internal static class RemoveSeriesErrorContent
{
    extension(RemoveSeriesError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError validation => validation.ToContent(),
            StorageError storage => storage.ToContent()
        };
    }
}
