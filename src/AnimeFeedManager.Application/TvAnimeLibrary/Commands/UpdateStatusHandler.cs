using System.Collections.Immutable;
using MediatR;

namespace AnimeFeedManager.Application.TvAnimeLibrary.Commands;

public record UpdateStatusCmd : IRequest<Either<DomainError, ImmutableList<AnimeInfoStorage>>>;

public class UpdateStatusHandler : IRequestHandler<UpdateStatusCmd, Either<DomainError, ImmutableList<AnimeInfoStorage>>>
{
    private readonly IFeedTitlesRepository _titlesRepository;
    private readonly IAnimeInfoRepository _animeInfoRepository;

    public UpdateStatusHandler(IFeedTitlesRepository titlesRepository, IAnimeInfoRepository animeInfoRepository)
    {
        _titlesRepository = titlesRepository;
        _animeInfoRepository = animeInfoRepository;
    }

    public Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> Handle(UpdateStatusCmd request, CancellationToken cancellationToken)
    {
        return _titlesRepository
            .GetTitles()
            .BindAsync(UpdateStatus);
    }

    private async Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> UpdateStatus(IImmutableList<string> animes)
    {
        return await _animeInfoRepository.GetIncomplete()
            .MapAsync(al => al.Where(x => !string.IsNullOrEmpty(x.FeedTitle) && !animes.Contains(x.FeedTitle)))
            .MapAsync(al => al.Select(MarkAsCompleted))
            .MapAsync(al => al.ToImmutableList());
    }

    private AnimeInfoStorage MarkAsCompleted(AnimeInfoStorage original)
    {
        original.Completed = true;
        return original;
    }

}