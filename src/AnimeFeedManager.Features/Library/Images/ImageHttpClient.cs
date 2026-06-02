namespace AnimeFeedManager.Features.Library.Images;

/// <summary>
/// Typed HTTP client for downloading series cover images. Exists as an interface purely
/// to satisfy typed-HttpClient DI registration
/// (<c>AddHttpClient&lt;IImageHttpClient, ImageHttpClient&gt;</c>) — the accepted exception
/// to the no-interfaces rule, mirroring <see cref="Import.Jikan.IJikanClient"/>.
/// </summary>
public interface IImageHttpClient
{
    /// <summary>
    /// Downloads the image at <paramref name="url"/> and returns its bytes. Throws on a
    /// non-success status. Covers are small, so the payload is buffered fully rather than
    /// streamed — this keeps the HTTP response lifetime entirely inside the client.
    /// </summary>
    Task<byte[]> DownloadImage(string url, CancellationToken cancellationToken);
}

/// <summary>
/// Thin concrete wrapper over the injected <see cref="HttpClient"/>. No base address or
/// headers — cover URLs are absolute Jikan CDN links, so it forwards the call directly and
/// owns the response lifetime, handing back only the image bytes.
/// </summary>
internal sealed class ImageHttpClient(HttpClient httpClient) : IImageHttpClient
{
    public async Task<byte[]> DownloadImage(string url, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
