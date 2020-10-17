using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Application.Feed.Queries
{
    public class GetLatestFeedHandler : IRequestHandler<GetLatestFeed, Either<DomainError, ImmutableList<FeedInfo>>>
    {
        private readonly IFeedProvider _feedProvider;
        private readonly IProcessedTitlesRepository _processedTitles;

        public GetLatestFeedHandler(IFeedProvider feedProvider, IProcessedTitlesRepository processedTitles)
        {
            _feedProvider = feedProvider;
            _processedTitles = processedTitles;
        }

        public Task<Either<DomainError, ImmutableList<FeedInfo>>> Handle(GetLatestFeed request,
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
}
