using MediatR;

namespace AnimeFeedManager.Features.Tv.Titles;

public readonly record struct UpdateSeasonTitles (ImmutableList<string> Titles): INotification;

public sealed class UpdateSeasonTitlesHandler : INotificationHandler<UpdateSeasonTitles>
{
    public Task Handle(UpdateSeasonTitles notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}