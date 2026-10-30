using AnimeFeedManager.Features.SystemEvents;

namespace AnimeFeedManager.Features.Tests.SystemEvents;

public class LiveAudienceTests
{
    [Fact]
    public void Should_Reach_Every_Connection_When_Consumer_Is_Everybody()
    {
        Assert.Equal(new EveryConnection(), LiveAudience.From(TargetConsumer.Everybody()));
    }

    [Fact]
    public void Should_Reach_Only_The_Admin_Group_When_Consumer_Is_Admin()
    {
        Assert.Equal(new GroupAudience(LiveAudience.AdminGroup), LiveAudience.From(TargetConsumer.Admin()));
    }

    [Fact]
    public void Should_Reach_Only_That_User_When_Consumer_Is_A_User()
    {
        Assert.Equal(new UserAudience("user-1"), LiveAudience.From(TargetConsumer.User("user-1")));
    }

    [Fact]
    public void Should_Reach_Nobody_When_Consumer_Is_None()
    {
        Assert.Equal(new NoAudience(), LiveAudience.From(TargetConsumer.None()));
    }

    [Fact]
    public void Should_Reach_Nobody_When_Consumer_Is_Unset()
    {
        Assert.Equal(new NoAudience(), LiveAudience.From(default));
    }
}
