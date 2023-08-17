namespace AnimeFeedManager.Features.Domain.Events;

public record DownloadImageEvent(string Partition, 
    string Id, 
    string Directory, 
    string BlobName, 
    string RemoteUrl,
    SeriesType SeriesType);
