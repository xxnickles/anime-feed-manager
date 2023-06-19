namespace AnimeFeedManager.Features
{
    public record DownloadImageEvent(string Partition, 
        string Id, 
        string Directory, 
        string BlobName, 
        string RemoteUrl,
        SeriesType SeriesType);
}