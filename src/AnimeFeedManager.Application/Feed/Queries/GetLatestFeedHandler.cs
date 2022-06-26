using System.Collections.Immutable;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;

namespace AnimeFeedManager.Application.Feed.Queries;

public sealed record GetLatestFeedQry(Resolution Resolution) : IRequest<Either<DomainError, ImmutableList<FeedInfo>>>;

public class GetLatestFeedHandler : IRequestHandler<GetLatestFeedQry, Either<DomainError, ImmutableList<FeedInfo>>>
{
    private readonly IFeedProvider _feedProvider;
    private readonly IProcessedTitlesRepository _processedTitles;

    public GetLatestFeedHandler(IFeedProvider feedProvider, IProcessedTitlesRepository processedTitles)
    {
        _feedProvider = feedProvider;
        _processedTitles = processedTitles;
    }

    public Task<Either<DomainError, ImmutableList<FeedInfo>>> Handle(GetLatestFeedQry request,
        CancellationToken cancellationToken)
    {
        return _processedTitles.GetProcessedTitles().BindAsync(titles => GetFilteredFeed(titles, Resolution.Hd));
    }


    private Either<DomainError, ImmutableList<FeedInfo>> GetFilteredFeed(IImmutableList<string> processedTitles, Resolution resolution)
    {
        return _feedProvider.GetFeed(resolution)
            .Map(titles =>
                titles.Where(title => 
                        !processedTitles.Exists(x => x == OptionUtils.UnpackOption(title.FeedTitle.Value, string.Empty)))
                    .ToImmutableList());
    }
}