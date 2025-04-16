﻿using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;

namespace AnimeFeedManager.Features.Users;

public class CleanAllSubscriptions
{
    private readonly IRemoveAllInterested _interestedRemover;
    private readonly IRemoveAllTvSubscriptions _tvSubscriptionsRemover;
    private readonly IRemoveAllMoviesSubscriptions _moviesSubscriptionsRemover;
    private readonly IRemoveAllOvasSubscriptions _ovasSubscriptionsRemover;

    public CleanAllSubscriptions(
        IRemoveAllInterested interestedRemover,
        IRemoveAllTvSubscriptions tvSubscriptionsRemover,
        IRemoveAllMoviesSubscriptions moviesSubscriptionsRemover,
        IRemoveAllOvasSubscriptions ovasSubscriptionsRemover)
    {
        _interestedRemover = interestedRemover;
        _tvSubscriptionsRemover = tvSubscriptionsRemover;
        _moviesSubscriptionsRemover = moviesSubscriptionsRemover;
        _ovasSubscriptionsRemover = ovasSubscriptionsRemover;
    }

    public async Task<Either<DomainError, ImmutableList<ProcessResult>>> CleanAll(UserId userId,
        CancellationToken token)
    {
        var interestedTask = _interestedRemover.UnsubscribeAll(userId, token);
        var tvSubscriptionsTask = _tvSubscriptionsRemover.UnsubscribeAll(userId, token);
        var moviesSubscriptionsTask = _moviesSubscriptionsRemover.UnsubscribeAll(userId, token);
        var ovasSubscriptionsTask = _ovasSubscriptionsRemover.UnsubscribeAll(userId, token);
        var results = await Task.WhenAll(interestedTask, tvSubscriptionsTask, moviesSubscriptionsTask,
            ovasSubscriptionsTask);
        return results.FlattenResults();
    }
}