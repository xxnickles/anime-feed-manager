using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Old.Features.Ovas.Subscriptions;

public sealed class OvasSubscriptionStatusResetter
{
    private readonly IGetOvasSubscriptions _ovaSubscriptions;
    private readonly IOvasSubscriptionStore _subscriptionStore;

    public OvasSubscriptionStatusResetter(
        IGetOvasSubscriptions  OvaSubscriptions,
        IOvasSubscriptionStore subscriptionStore)
    {
        _ovaSubscriptions = OvaSubscriptions;
        _subscriptionStore = subscriptionStore;
    }

    public Task<Either<DomainError, Unit>> ResetStatus(RowKey series, CancellationToken token)
    {
        return _ovaSubscriptions.GetSubscriptionForOva(series,token)
            .MapAsync(OvaSubscriptions => OvaSubscriptions.ConvertAll(ResetSeriesStatus))
            .BindAsync(processedOvas => _subscriptionStore.BulkUpdate(processedOvas, token));
    }


    private OvasSubscriptionStorage ResetSeriesStatus(OvasSubscriptionStorage OvasSubscriptionStorage)
    {
        OvasSubscriptionStorage.Processed = false;
        return OvasSubscriptionStorage;
    }
}