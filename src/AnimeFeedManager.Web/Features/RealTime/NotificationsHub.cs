using AnimeFeedManager.Services.Shared;
using Microsoft.Azure.SignalR.Management;

namespace AnimeFeedManager.Web.Features.RealTime;

/// <summary>The notifications hub context, created on first use and shared for the app's lifetime.</summary>
internal sealed class NotificationsHub(ServiceManager serviceManager)
{
    private Task<ServiceHubContext>? _context;

    internal async Task<Result<ServiceHubContext>> GetContext()
    {
        try
        {
            return await (_context ??= serviceManager.CreateHubContextAsync(HubNames.Notifications, CancellationToken.None));
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // Dropped so the next negotiate retries the creation.
            _context = null;
            return ExceptionError.FromExceptionWithMessage(e, "The notifications hub context could not be created");
        }
    }
}
