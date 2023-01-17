using System.Diagnostics;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Common.Notifications.Realtime;
using AnimeFeedManager.Storage.Infrastructure;
using MediatR;
using NotificationType = AnimeFeedManager.Common.Notifications.NotificationType;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.State.Commands;

public record UpdateMovieScrapStateCmd
    (string StateId, UpdateType Type, SeasonInfoDto SeasonInfo) : IRequest<Either<DomainError, Unit>>;

public class UpdateMovieScrapStateHandler : IRequestHandler<UpdateMovieScrapStateCmd, Either<DomainError, Unit>>
{
    private readonly IUpdateState _updateState;
    private readonly IDomainPostman _domainPostman;
    private readonly INotificationsRepository _repository;

    public UpdateMovieScrapStateHandler(
        IUpdateState updateState,
        IDomainPostman domainPostman,
        INotificationsRepository repository)
    {
        _updateState = updateState;
        _domainPostman = domainPostman;
        _repository = repository;
    }

    public Task<Either<DomainError, Unit>> Handle(UpdateMovieScrapStateCmd request, CancellationToken cancellationToken)
    {
        return (request.Type switch
            {
                UpdateType.Complete => UpdateCompletedState(request.StateId),
                UpdateType.Error => UpdateErrorState(request.StateId),
                _ => throw new UnreachableException($"'{nameof(request.Type)}' Should not have invalid value")
            })
            .BindAsync(_ => GetLastState(request.StateId))
            .BindAsync(nr => CheckState(nr, request.SeasonInfo.Season, request.SeasonInfo.Year));
    }

    private Task<Either<DomainError, NotificationResult>> GetLastState(string id)
    {
        // Wait a little to catch any other updated
        Task.Delay(500);
        return _updateState.GetCurrent(id, NotificationType.Movie);
    }


    private Task<Either<DomainError, Unit>> UpdateCompletedState(string stateId)
    {
        return _updateState.AddComplete(stateId, NotificationType.Movie);
    }

    private Task<Either<DomainError, Unit>> UpdateErrorState(string stateId)
    {
        return _updateState.AddError(stateId, NotificationType.Movie);
    }

    private async Task<Either<DomainError, Unit>> CheckState(NotificationResult result, string season, int year)
    {
        if (!result.IsCompleted) return new Unit();

        await _domainPostman.SendMessage(new SeasonProcessNotification(
            IdHelpers.GetUniqueId(),
            TargetAudience.All,
            Common.Notifications.Realtime.NotificationType.Update,
            new SeasonInfoDto(season, year),
            SeriesType.Movie,
            $"Season information for {season}-{year} has been updated"));


        await _domainPostman.SendMessage(new SeasonProcessNotification(
            IdHelpers.GetUniqueId(),
            TargetAudience.Admins,
            Common.Notifications.Realtime.NotificationType.Information,
            new SeasonInfoDto(season, year),
            SeriesType.Movie,
            $"Movie series has been updated. [{result.Completed}] Completed [{result.Errors}] Errors"));

        return await _repository.Merge(UserRoles.Admin, NotificationType.Movie,
            new UpdateNotification(result.Completed, result.Errors));
    }
}