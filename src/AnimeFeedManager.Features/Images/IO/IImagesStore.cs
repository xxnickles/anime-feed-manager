namespace AnimeFeedManager.Features.Images.IO;

public interface IImagesStore
{
    public Task<Uri> Upload(string fileName, string path, Stream data);
}