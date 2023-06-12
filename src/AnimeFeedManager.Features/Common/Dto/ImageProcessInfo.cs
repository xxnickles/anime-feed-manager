namespace AnimeFeedManager.Features.Common.Dto;

public class BlobImageInfoEvent
{
    public string Partition { get; }
    public string Id { get; }
    public string Directory { get; }
    public string BlobName { get; }
    public string RemoteUrl { get; }
    public SeriesType SeriesType { get; }

    public BlobImageInfoEvent(
        string partition, 
        string id, 
        string directory, 
        string blobName, 
        string remoteUrl,
        SeriesType seriesType)
    {

        Directory = directory;
        BlobName = blobName;
        RemoteUrl = remoteUrl;
        SeriesType = seriesType;
        Partition = partition;
        Id = id;
    }
}