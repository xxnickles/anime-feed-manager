namespace AnimeFeedManager.Features.Scrapping.SubsPlease;

public sealed record ScheduledShow(string Title);

public interface ISubsPleaseSchedule
{
    Task<Result<ImmutableArray<ScheduledShow>>> Fetch(CancellationToken token = default);
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

    public Task<Result<ImmutableArray<ScheduledShow>>> Fetch(CancellationToken token = default) =>
        FetchSchedule(token)
            .Bind(ToShows)
            .WithOperationName(nameof(Fetch))
            .WithLogProperty(ScrappingTelemetry.QueryTag, Request);

    private async Task<Result<SubsPleaseScheduleResponse>> FetchSchedule(CancellationToken token)
    {
        try
        {
            using var response = await _httpClient.GetAsync(Request, token);

            // Checked before the status: a challenge page can arrive as a 200 or as an error.
            if (response.Content.Headers.ContentType?.MediaType is "text/html")
                return ScheduleUnavailableError.Create(Request, response.StatusCode);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var payload = await JsonSerializer.DeserializeAsync(
                stream, SubsPleaseJsonContext.Default.SubsPleaseScheduleResponse, token);

            return payload is not null
                ? payload
                : Error.Create($"No usable payload for {Request}");
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromExceptionWithMessage(e, $"Request failed for {Request}");
        }
    }

    private static Result<ImmutableArray<ScheduledShow>> ToShows(SubsPleaseScheduleResponse response)
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
