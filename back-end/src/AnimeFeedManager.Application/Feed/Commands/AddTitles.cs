using System.Collections.Generic;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.Feed.Commands;

public class AddTitles : Record<AddTitles>, IRequest<Either<DomainError, Unit>>
{
    public IEnumerable<string> Titles { get; }


    public AddTitles(IEnumerable<string> titles)
    {
        Titles = titles;
    }
}