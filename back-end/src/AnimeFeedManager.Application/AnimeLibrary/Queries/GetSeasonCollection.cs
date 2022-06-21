using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public sealed class GetSeasonCollection : Record<GetSeasonCollection>, IRequest<Either<DomainError, SeasonCollection>>
{
    public string Season { get; }
    public ushort Year { get; }

    public GetSeasonCollection(string season, ushort year)
    {
        Season = season;
        Year = year;
    }
}