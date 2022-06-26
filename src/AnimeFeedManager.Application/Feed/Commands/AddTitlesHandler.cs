using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Application.Feed.Commands;

public record AddTitlesCmd(IEnumerable<string> Titles) : MediatR.IRequest<Either<DomainError, Unit>>;

public class AddTitlesHandler : MediatR.IRequestHandler<AddTitlesCmd, Either<DomainError, Unit>>
{
    private readonly IFeedTitlesRepository _titlesRepository;

    public AddTitlesHandler(IFeedTitlesRepository titlesRepository)
    {
        _titlesRepository = titlesRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(AddTitlesCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(AddTitlesCmd))
            .BindAsync(Save);
    }

    private Validation<ValidationError, IEnumerable<string>> Validate(AddTitlesCmd param)
    {
        if (param.Titles.Any()) return Success<ValidationError, IEnumerable<string>>(param.Titles);

        var error = ValidationError.Create(nameof(AddTitlesCmd.Titles), new[] { "Titles collection is empty" });
        return Fail<ValidationError, IEnumerable<string>>(error);
    }

    private Task<Either<DomainError, Unit>> Save(IEnumerable<string> titles)
    {
        return _titlesRepository.MergeTitles(titles);
    }
}