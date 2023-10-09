namespace AnimeFeedManager.Features.Images.IO
{
    public interface IImagesBlobStore
    {
        public Task<Uri> Upload(string fileName, string path, Stream data);
    }
}