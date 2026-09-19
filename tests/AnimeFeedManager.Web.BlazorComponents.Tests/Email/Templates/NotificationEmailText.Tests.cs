namespace AnimeFeedManager.Web.BlazorComponents.Tests.Email.Templates;

public class NotificationEmailTextTests
{
    [Fact]
    public void Should_List_Episodes_In_Model_Order()
    {
        var text = NotificationEmailText.Render(EmailModels.ForEpisodes("101", "100", "99"));

        Assert.Equal(["101", "100", "99"], EmailRender.EpisodeNumbersInOrder(text));
    }

    [Fact]
    public void Should_Not_Reorder_Episodes_When_Model_Order_Is_Unsorted()
    {
        var text = NotificationEmailText.Render(EmailModels.ForEpisodes("99", "101", "100"));

        Assert.Equal(["99", "101", "100"], EmailRender.EpisodeNumbersInOrder(text));
    }

    [Fact]
    public void Should_Render_No_Episodes_When_Model_Has_None()
    {
        var text = NotificationEmailText.Render(EmailModels.Notification());

        Assert.Empty(EmailRender.EpisodeNumbersInOrder(text));
    }

    [Fact]
    public async Task Should_List_Episodes_In_The_Same_Order_As_The_Html_Part()
    {
        var model = EmailModels.Notification(
            EmailModels.Series("First Series", "10", "08"),
            EmailModels.Series("Second Series", "02v2", "02"));

        var text = NotificationEmailText.Render(model);
        var html = await EmailRender.Html(model);

        Assert.Equal(
            EmailRender.EpisodeNumbersInOrder(html),
            EmailRender.EpisodeNumbersInOrder(text));
    }
}
