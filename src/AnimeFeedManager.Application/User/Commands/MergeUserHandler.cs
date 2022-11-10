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
            .BindAsync(CheckIfExist)
            .BindAsync(MergeUser);
    }

    private Either<DomainError, UserStorage> Validate(MergeUserCmd request)
    {
        return Email.FromString(request.Email)
            .ToValidation(ValidationError.Create(nameof(MergeUserCmd.Email),
                new[] { "A valid email address must be provided" }))
            .Map(e => new UserStorage
            {
                Email = e.Value.UnpackOption(string.Empty),
                RowKey = request.Id,
                PartitionKey = "user-group"
            }).ToEither(nameof(MergeUserCmd));
    }

    private async Task<Either<DomainError, UserStorage>> CheckIfExist(UserStorage user)
    { 
        var result = await _userRepository.GetUserId(user.Email ?? string.Empty);

        Option<UserStorage> Evaluate(Option<string> target)
        {
            return target.Match(
                _ => Option<UserStorage>.None,
                () => Some(user)
            );
        }

        return result.Bind(
            id => Evaluate(id)
                .ToEither((DomainError)ValidationErrors.Create(nameof(MergeUserCmd), new[]
                {
                    new ValidationError("Email", new[] { "Email is already registered in the system" })
                }))
        );
    }

    private Task<Either<DomainError, Unit>> MergeUser(UserStorage user)
    {
        return _userRepository.MergeUser(user);
    }
}