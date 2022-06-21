using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.Feed.Commands;

public class AddTitlesHandler : IRequestHandler<AddTitles, Either<DomainError, LanguageExt.Unit>>
{
    private readonly IFeedTitlesRepository _titlesRepository;

    public AddTitlesHandler(IFeedTitlesRepository titlesRepository)
    {
        _titlesRepository = titlesRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(AddTitles request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(AddTitles))
            .BindAsync(Save);
    }

    private Validation<ValidationError, IEnumerable<string>> Validate(AddTitles param)
    {
        if (param.Titles.Any()) return Success<ValidationError, IEnumerable<string>>(param.Titles);

        var error = ValidationError.Create(nameof(AddTitles.Titles), new[] { "Titles collection is empty" });
        return Fail<ValidationError, IEnumerable<string>>(error);
    }

    private Task<Either<DomainError, Unit>> Save(IEnumerable<string> titles)
    {
        return _titlesRepository.MergeTitles(titles);
    }
}