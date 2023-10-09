using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using SimpleTvSubscriptionContext = AnimeFeedManager.Common.Dto.SimpleTvSubscriptionContext;

namespace AnimeFeedManager.Functions.Tv.Subscriptions
{
    public class AddInterested
    {
        private readonly IAddInterested _addInterested;
        private readonly ILogger<AddInterested> _logger;

        public AddInterested(
            IAddInterested addInterested,
            ILoggerFactory loggerFactory)
        {
            _addInterested = addInterested;
            _logger = loggerFactory.CreateLogger<AddInterested>();
        }

        [Function("AddTvInterested")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "tv/interested")]
            HttpRequestData req)
        {
            var payload =
                await JsonSerializer.DeserializeAsync(req.Body, SimpleTvSubscriptionContext.Default.SimpleTvSubscription);
            ArgumentNullException.ThrowIfNull(payload);

            return await Utils.Validate(payload)
                .BindAsync(param => _addInterested.Add(param.UserId, param.Series, default))
                .ToResponse(req, _logger);
        }
    }
}