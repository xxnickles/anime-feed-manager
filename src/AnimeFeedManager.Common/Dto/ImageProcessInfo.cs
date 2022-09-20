namespace AnimeFeedManager.Functions.Models;

public class BlobImageInfoEvent
{
    public string Partition { get; }
    public string Id { get; }
    public string Directory { get; }
    public string BlobName { get; }
    public string RemoteUrl { get; }

    public BlobImageInfoEvent(
        string partition, 
        string id, 
        string directory, 
        string blobName, 
        string remoteUrl)
    {

        Directory = directory;
        BlobName = blobName;
        RemoteUrl = remoteUrl;
        Partition = partition;
        Id = id;
    }
}