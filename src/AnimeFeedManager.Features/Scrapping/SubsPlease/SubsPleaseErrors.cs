using System.Net;

namespace AnimeFeedManager.Features.Scrapping.SubsPlease;

/// <summary>An HTML page in place of the schedule JSON: a challenge page or a site change.</summary>
internal sealed record ScheduleUnavailableError : DomainError
{
    private ScheduleUnavailableError(string message) : base(message)
    {
    }

    public static ScheduleUnavailableError Create(string request, HttpStatusCode status) =>
        new($"SubsPlease answered {request} with an HTML page (status {(int) status})");

    public override Action<ILogger> LogAction() =>
        logger => logger.LogError("{domain_error.message}", Message);
}
