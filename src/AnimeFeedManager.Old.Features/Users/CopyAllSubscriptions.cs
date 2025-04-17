using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.IO;

namespace AnimeFeedManager.Old.Features.Users;

public class CopyAllSubscriptions
{
    private readonly ICopyInterested _interestedCopier;
    private readonly ICopyTvSubscriptions _tvSubscriptionsCopier;
    private readonly ICopyMoviesSubscriptions _moviesSubscriptionsCopier;
    private readonly ICopyOvasSubscriptions _ovasSubscriptionsCopier;

    public CopyAllSubscriptions(
        ICopyInterested interestedCopier,
        ICopyTvSubscriptions tvSubscriptionsCopier,
        ICopyMoviesSubscriptions moviesSubscriptionsCopier,
        ICopyOvasSubscriptions ovasSubscriptionsCopier)
    {
        _interestedCopier = interestedCopier;
        _tvSubscriptionsCopier = tvSubscriptionsCopier;
        _moviesSubscriptionsCopier = moviesSubscriptionsCopier;
        _ovasSubscriptionsCopier = ovasSubscriptionsCopier;
    }

    public async Task<Either<DomainError, ImmutableList<ProcessResult>>> CopyAll(UserId source, UserId target,
        CancellationToken token)
    {
        var interestedTask = _interestedCopier.CopyAll(source,target, token);
        var tvSubscriptionsTask = _tvSubscriptionsCopier.CopyAll(source,target, token);
        var moviesSubscriptionsTask = _moviesSubscriptionsCopier.CopyAll(source,target, token);
        var ovasSubscriptionsTask = _ovasSubscriptionsCopier.CopyAll(source,target, token);
        var results = await Task.WhenAll(interestedTask, tvSubscriptionsTask, moviesSubscriptionsTask,
            ovasSubscriptionsTask);
        return results.FlattenResults();
    }
}