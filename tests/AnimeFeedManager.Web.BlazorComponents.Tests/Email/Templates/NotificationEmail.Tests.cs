namespace AnimeFeedManager.Web.BlazorComponents.Tests.Email.Templates;

public class NotificationEmailTests
{
    [Fact]
    public async Task Should_List_Episodes_In_Model_Order()
    {
        var html = await EmailRender.Html(EmailModels.ForEpisodes("101", "100", "99"));

        Assert.Equal(["101", "100", "99"], EmailRender.EpisodeNumbersInOrder(html));
    }

    [Fact]
    public async Task Should_Not_Reorder_Episodes_When_Model_Order_Is_Unsorted()
    {
        var html = await EmailRender.Html(EmailModels.ForEpisodes("99", "101", "100"));

        Assert.Equal(["99", "101", "100"], EmailRender.EpisodeNumbersInOrder(html));
    }

    [Fact]
    public async Task Should_List_Every_Series_When_Model_Has_Several()
    {
        var model = EmailModels.Notification(
            EmailModels.Series("First Series", "10", "08"),
            EmailModels.Series("Second Series", "02v2", "02"));

        var html = await EmailRender.Html(model);

        Assert.Equal(["10", "08", "02v2", "02"], EmailRender.EpisodeNumbersInOrder(html));
    }

    [Fact]
    public async Task Should_Render_No_Episodes_When_Model_Has_None()
    {
        var html = await EmailRender.Html(EmailModels.Notification());

        Assert.Empty(EmailRender.EpisodeNumbersInOrder(html));
    }
}
