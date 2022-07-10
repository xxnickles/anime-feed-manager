using MediatR;

namespace AnimeFeedManager.Application.User.Queries;

public record GetUserEmailQry(string Id) : IRequest<Either<DomainError, string>>;

public class GetUserEmailHandler : IRequestHandler<GetUserEmailQry, Either<DomainError, string>>
{
    private readonly IUserRepository _userRepository;

    public GetUserEmailHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<Either<DomainError, string>> Handle(GetUserEmailQry request, CancellationToken cancellationToken)
    {
        return _userRepository.GetUserEmail(request.Id);
    }
}