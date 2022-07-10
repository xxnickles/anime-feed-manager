using AnimeFeedManager.Core.Utils;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.User.Commands;

public record MergeUserCmd(string Id, string Email) : IRequest<Either<DomainError, Unit>>;

public class MergeUserHandler : IRequestHandler<MergeUserCmd, Either<DomainError, Unit>>
{
    private readonly IUserRepository _userRepository;

    public MergeUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(MergeUserCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(MergeUserCmd))
            .BindAsync(MergeUser);
    }

    private Validation<ValidationError, UserStorage> Validate(MergeUserCmd request)
    {
        return Email.FromString(request.Email)
            .ToValidation(ValidationError.Create(nameof(MergeUserCmd.Email),
                new[] {"A valid email address must be provided"}))
            .Map(e => new UserStorage
            {
                Email = OptionUtils.UnpackOption(e.Value, string.Empty),
                RowKey = request.Id,
                PartitionKey = "user-group",
            });
    }

    private Task<Either<DomainError, Unit>> MergeUser(UserStorage user)
    {
        return _userRepository.MergeUser(user);
    }
}