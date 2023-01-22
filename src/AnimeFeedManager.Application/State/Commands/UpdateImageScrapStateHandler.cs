using System.Diagnostics;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Common.Notifications.Realtime;
using AnimeFeedManager.Storage.Infrastructure;
using MediatR;
using NotificationType = AnimeFeedManager.Common.Notifications.NotificationType;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.State.Commands;

public record UpdateImageScrapStateCmd(
    string StateId,
    SeriesType SeriesType,
    UpdateType Type,
    string Path) : IRequest<Either<DomainError, Unit>>;

public class UpdateImageScrapStateHandler : IRequestHandler<UpdateImageScrapStateCmd, Either<DomainError, Unit>>
{
    private readonly IUpdateState _updateState;
    private readonly IDomainPostman _domainPostman;
    private readonly INotificationsRepository _repository;

    public UpdateImageScrapStateHandler(
        IUpdateState updateState,
        IDomainPostman domainPostman,
        INotificationsRepository repository)
    {
        _updateState = updateState;
        _domainPostman = domainPostman;
        _repository = repository;
    }

    public Task<Either<DomainError, Unit>> Handle(UpdateImageScrapStateCmd request, CancellationToken cancellationToken)
    {
        return (request.Type switch
            {
                UpdateType.Complete => UpdateCompletedState(request.StateId),
                UpdateType.Error => UpdateErrorState(request.StateId),
                _ => throw new UnreachableException($"'{nameof(request.Type)}' Should not have invalid value")
            })
            .BindAsync(nr => CheckState(nr, request.SeriesType, request.Path));
    }

    private Task<Either<DomainError, NotificationResult>> UpdateCompletedState(string stateId)
    {
        return _updateState.AddComplete(stateId, NotificationType.Images);
    }

    private Task<Either<DomainError, NotificationResult>> UpdateErrorState(string stateId)
    {
        return _updateState.AddError(stateId, NotificationType.Images);
    }

    private async Task<Either<DomainError, Unit>> CheckState(NotificationResult result, SeriesType seriesType,
        string path)
    {
        if (!result.ShouldNotify) return new Unit();

        await _domainPostman.SendMessage(new ImageUpdateNotification(
            IdHelpers.GetUniqueId(),
            Common.Notifications.Realtime.NotificationType.Information,
            seriesType,
            $"Images for {seriesType} have been scrapped. Completed: {result.Completed} Errors: {result.Errors}"));

        return await _repository.Merge(result.Id, UserRoles.Admin, NotificationType.Images,
            new UpdateNotification(result.Completed, result.Errors));
    }
}