using System.Xml;
using System.Xml.Linq;

namespace AnimeFeedManager.Features.Scrapping.Nyaa;

public interface INyaaClient
{
    Task<Result<XDocument>> Fetch(NyaaQuery query, CancellationToken token = default);
}

internal sealed class NyaaClient : INyaaClient
{
    // Untrusted remote XML: no DTDs.
    private static readonly XmlReaderSettings ReaderSettings = new()
    {
        Async = true,
        DtdProcessing = DtdProcessing.Prohibit
    };

    private readonly HttpClient _httpClient;

    public NyaaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<Result<XDocument>> Fetch(NyaaQuery query, CancellationToken token = default)
    {
        var request = query.Build();
        return FetchFeed(request, token)
            .WithOperationName(nameof(Fetch))
            .WithLogProperty(ScrappingTelemetry.QueryTag, request);
    }

    private async Task<Result<XDocument>> FetchFeed(string request, CancellationToken token)
    {
        try
        {
            using var response = await _httpClient.GetAsync(request, token);

            // Checked before the status: the challenge page can arrive as a 200 or as an error.
            if (response.Content.Headers.ContentType?.MediaType is "text/html")
                return NyaaChallengeError.Create(request, response.StatusCode);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            using var reader = XmlReader.Create(stream, ReaderSettings);
            return await XDocument.LoadAsync(reader, LoadOptions.None, token);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromExceptionWithMessage(e, $"Request failed for {request}");
        }
    }
}
