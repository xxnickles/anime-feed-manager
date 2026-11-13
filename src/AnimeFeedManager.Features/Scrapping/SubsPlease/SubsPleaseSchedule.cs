namespace AnimeFeedManager.Features.Scrapping.SubsPlease;

public sealed record ScheduledShow(string Title);

public interface ISubsPleaseSchedule
{
    Task<Result<ImmutableArray<ScheduledShow>, DomainError>> Fetch(CancellationToken token = default);
}

/// <summary>Reads the unofficial JSON endpoint behind the site's schedule page.</summary>
internal sealed class SubsPleaseSchedule : ISubsPleaseSchedule
{
    private const string Request = "api/?f=schedule&tz=UTC";

    private readonly HttpClient _httpClient;

    public SubsPleaseSchedule(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<Result<ImmutableArray<ScheduledShow>, DomainError>> Fetch(CancellationToken token = default) =>
        FetchSchedule(token)
            .MapError(error => (DomainError)error)
            .Bind(ToShows)
            .WithOperationName(nameof(Fetch))
            .WithLogProperty(ScrappingTelemetry.QueryTag, Request);

    private async Task<Result<SubsPleaseScheduleResponse, ScrapeError>> FetchSchedule(CancellationToken token)
    {
        try
        {
            using var response = await _httpClient.GetAsync(Request, token);
            return await Parse(response, token);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return new ScrapeError.RequestFailed(Request, e);
        }
    }

    // The body decides, not Content-Type (the JSON is served as text/html) or the status
    // (a challenge page can arrive as a 200 or as an error).
    private static async Task<Result<SubsPleaseScheduleResponse, ScrapeError>> Parse(HttpResponseMessage response,
        CancellationToken token)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var payload = await JsonSerializer.DeserializeAsync(
                stream, SubsPleaseJsonContext.Default.SubsPleaseScheduleResponse, token);

            response.EnsureSuccessStatusCode();

            return payload is not null
                ? payload
                : new ScrapeError.UnusablePayload(Request);
        }
        catch (JsonException)
        {
            return new ScrapeError.SourceUnavailable(Request, response.StatusCode);
        }
    }

    private static Result<ImmutableArray<ScheduledShow>, DomainError> ToShows(SubsPleaseScheduleResponse response)
    {
        var shows = response.Schedule.Values
            .SelectMany(day => day)
            .Select(entry => entry.Title.Trim())
            .Where(title => title.Length > 0)
            .Distinct()
            .Select(title => new ScheduledShow(title))
            .ToImmutableArray();

        return shows.IsEmpty
            ? Warning.Create($"SubsPlease {Request} listed no shows")
            : shows;
    }
}
