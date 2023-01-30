using AnimeFeedManager.Common.Dto;
using MediatR;

namespace AnimeFeedManager.Application.Notifications.Queries;

public sealed record GetUserNotificationsQry(string UserId): IRequest<Either<DomainError, UiNotifications>>;

public class  GetUserNotificationsHandler : IRequestHandler<GetUserNotificationsQry, Either<DomainError, UiNotifications>>
{
    private readonly INotificationsRepository _repository;

    public GetUserNotificationsHandler(INotificationsRepository repository)
    {
        _repository = repository;
    }
    
    public Task<Either<DomainError, UiNotifications>> Handle(GetUserNotificationsQry request, CancellationToken cancellationToken)
    {
        return _repository.GetForUser(request.UserId).MapAsync(Mappers.Map);
    }
}