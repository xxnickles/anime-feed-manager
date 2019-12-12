using System.Collections.Generic;

namespace AnimeFeedManager.Functions.Models
{
    public class ImageInfo
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
    }

    public class ImageProcessInfo
    {
        public SeasonInfo? SeasonInfo { get; set; }
        public IEnumerable<ImageInfo>? ImagesInfo { get; set; }
    }

    public class BlobImageInfo
    {
        public string Partition { get; }
        public string Id { get; }
        public string Directory { get; }
        public string BlobName { get; }
        public string RemoteUrl { get; }

        public BlobImageInfo(
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
}
