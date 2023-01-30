using AnimeFeedManager.Common.Dto;
using MediatR;

namespace AnimeFeedManager.Application.Notifications.Queries;

public sealed record GetAdminNotificationsQry(string UserId): IRequest<Either<DomainError, UiNotifications>>;

public class GetAdminNotificationsHandler : IRequestHandler<GetAdminNotificationsQry, Either<DomainError, UiNotifications>>
{
    private readonly INotificationsRepository _repository;

    public GetAdminNotificationsHandler(INotificationsRepository repository)
    {
        _repository = repository;
    }
    
    public Task<Either<DomainError, UiNotifications>> Handle(GetAdminNotificationsQry request, CancellationToken cancellationToken)
    {
        return _repository.GetForAdmin(request.UserId).MapAsync(Mappers.Map);
    }
}