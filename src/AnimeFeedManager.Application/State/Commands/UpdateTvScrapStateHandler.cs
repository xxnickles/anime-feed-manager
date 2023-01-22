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

public record UpdateTvScrapStateCmd
    (string StateId, UpdateType Type, SeasonInfoDto SeasonInfo) : IRequest<Either<DomainError, Unit>>;

public class UpdateTvScrapStateHandler : IRequestHandler<UpdateTvScrapStateCmd, Either<DomainError, Unit>>
{
    private readonly IUpdateState _updateState;
    private readonly IDomainPostman _domainPostman;
    private readonly INotificationsRepository _repository;

    public UpdateTvScrapStateHandler(
        IUpdateState updateState,
        IDomainPostman domainPostman,
        INotificationsRepository repository)
    {
        _updateState = updateState;
        _domainPostman = domainPostman;
        _repository = repository;
    }

    public Task<Either<DomainError, Unit>> Handle(UpdateTvScrapStateCmd request, CancellationToken cancellationToken)
    {
        return (request.Type switch
            {
                UpdateType.Complete => UpdateCompletedState(request.StateId),
                UpdateType.Error => UpdateErrorState(request.StateId),
                _ => throw new UnreachableException($"'{nameof(request.Type)}' Should not have invalid value")
            })
            .BindAsync(nr => CheckState(nr, request.SeasonInfo.Season, request.SeasonInfo.Year));
    }

    private Task<Either<DomainError, NotificationResult >> UpdateCompletedState(string stateId)
    {
        return _updateState.AddComplete(stateId, NotificationType.Tv);
    }

    private Task<Either<DomainError, NotificationResult>> UpdateErrorState(string stateId)
    {
        return _updateState.AddError(stateId, NotificationType.Tv);
    }

    private async Task<Either<DomainError, Unit>> CheckState(NotificationResult result, string season, int year)
    {
        if (!result.ShouldNotify) return new Unit();

        await _domainPostman.SendMessage(new SeasonProcessNotification(
            IdHelpers.GetUniqueId(),
            TargetAudience.All,
            Common.Notifications.Realtime.NotificationType.Update,
            new SeasonInfoDto(season, year),
            SeriesType.Tv,
            $"Season information for {season}-{year} has been updated"));


        await _domainPostman.SendMessage(new SeasonProcessNotification(
            IdHelpers.GetUniqueId(),
            TargetAudience.Admins,
            Common.Notifications.Realtime.NotificationType.Information,
            new SeasonInfoDto(season, year),
            SeriesType.Tv,
            $"TV series has been updated. [Completed]: {result.Completed} [Errors]: {result.Errors}"));

        return await _repository.Merge(result.Id, UserRoles.Admin, NotificationType.Tv,
            new UpdateNotification(result.Completed, result.Errors));
    }
}