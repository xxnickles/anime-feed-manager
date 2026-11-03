using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.AutomatedBinding;

public class ScheduleMatchingTests
{
    #region Match Tests

    [Fact]
    public void Should_Match_By_Feed_Title_When_Series_Was_Fed_Before()
    {
        var result = ScheduleMatching.Match(
            [Show("Kusuriya no Hitorigoto")],
            [Candidate("a", feedTitle: "Kusuriya no Hitorigoto", "The Apothecary Diaries")]);

        Assert.Equal([new ScheduleMatch("Kusuriya no Hitorigoto", "a")], result.Matches);
    }

    [Theory]
    [InlineData("PSYREN", "Psyren")]
    [InlineData("Tensei Goblin dakedo Shitsumon Aru", "Tensei Goblin dakedo Shitsumon Aru?")]
    [InlineData("Tensei Shitara Ken Deshita S2", "Tensei shitara Ken deshita 2")]
    [InlineData("Hyouken no Majutsushi ga Sekai wo Suberu S2", "Hyouken no Majutsushi ga Sekai wo Suberu II")]
    [InlineData("Kusuriya no Hitorigoto", "Kusuriya no Hitorigoto 3rd Season")]
    [InlineData("Kyouran Reijou Nia Liston", "Kyouran Reijou Nia Liston: Byoujaku Reijou ni Tensei shita Kamigoroshi no Bujin no Karei Naru Musouroku")]
    [InlineData("Magical Explorer", "Magical★Explorer: Eroge no Yuujin Chara ni Tensei Shitakedo, Game Chishiki Tsukatte Jiyuu ni Ikiru")]
    public void Should_Match_When_Schedule_Spells_Or_Shortens_The_Title(string show, string title)
    {
        var result = ScheduleMatching.Match([Show(show)], [Candidate("a", feedTitle: "", title)]);

        Assert.Equal([new ScheduleMatch(show, "a")], result.Matches);
    }

    [Fact]
    public void Should_Match_By_Synonym_When_Schedule_Uses_Another_Title()
    {
        var result = ScheduleMatching.Match(
            [Show("Tempal - Item no Chikara")],
            [Candidate("a", feedTitle: "", "OVERGEARED", "Tempal - Item no Chikara")]);

        Assert.Equal([new ScheduleMatch("Tempal - Item no Chikara", "a")], result.Matches);
    }

    [Fact]
    public void Should_Not_Match_When_Titles_Are_Different_Series()
    {
        var result = ScheduleMatching.Match(
            [Show("Tsuihou sareta Tensei Juukishi wa Game Chishiki de Musou suru")],
            [Candidate("a", feedTitle: "", "Tsuihou sareta Cheat Fuyo Majutsushi wa Kimama na Second Life wo Ouka suru")]);

        Assert.Empty(result.Matches);
    }

    [Theory]
    [InlineData("Ao Ashi S2", "Ao Ashi", "Ao Ashi 2nd Season")]
    [InlineData("Kingdom S6", "Kingdom 5th Season", "Kingdom 6th Season")]
    public void Should_Prefer_The_Sequel_When_The_Previous_Season_Is_Also_A_Candidate(
        string show, string previous, string sequel)
    {
        var result = ScheduleMatching.Match(
            [Show(show)],
            [Candidate("previous", feedTitle: "", previous), Candidate("sequel", feedTitle: "", sequel)]);

        Assert.Equal([new ScheduleMatch(show, "sequel")], result.Matches);
        Assert.Empty(result.Ties);
    }

    [Fact]
    public void Should_Give_Candidate_To_Best_Score_When_Two_Shows_Claim_It()
    {
        var result = ScheduleMatching.Match(
            [Show("Kusuriya no Hitorigoto"), Show("Kusuriya no Hitorigoto S3")],
            [Candidate("a", feedTitle: "", "Kusuriya no Hitorigoto 3rd Season")]);

        Assert.Equal([new ScheduleMatch("Kusuriya no Hitorigoto S3", "a")], result.Matches);
        Assert.Empty(result.Ties);
    }

    #endregion

    #region Tie Tests

    [Fact]
    public void Should_Tie_When_Candidates_Share_A_Show_Best_Score()
    {
        var result = ScheduleMatching.Match(
            [Show("Kusuriya no Hitorigoto")],
            [Candidate("a", feedTitle: "", "Kusuriya no Hitorigoto 2nd Season"),
             Candidate("b", feedTitle: "", "Kusuriya no Hitorigoto 3rd Season")]);

        Assert.Empty(result.Matches);
        Assert.Equal("Kusuriya no Hitorigoto", Assert.Single(result.Ties).ShowTitle);
        Assert.Equal(["a", "b"], result.Ties[0].SeriesIds);
    }

    [Fact]
    public void Should_Tie_When_Shows_Share_A_Candidate_Best_Score()
    {
        var result = ScheduleMatching.Match(
            [Show("Romelia Senki"), Show("ROMELIA SENKI")],
            [Candidate("a", feedTitle: "", "Romelia Senki")]);

        Assert.Empty(result.Matches);
        Assert.Equal(["Romelia Senki", "ROMELIA SENKI"], result.Ties.Select(tie => tie.ShowTitle));
    }

    #endregion

    #region Empty Input Tests

    [Fact]
    public void Should_Return_Nothing_When_There_Are_No_Candidates()
    {
        var result = ScheduleMatching.Match([Show("Romelia Senki")], []);

        Assert.Empty(result.Matches);
        Assert.Empty(result.Ties);
    }

    [Fact]
    public void Should_Skip_Candidate_When_It_Has_No_Titles()
    {
        var result = ScheduleMatching.Match([Show("Romelia Senki")], [Candidate("a", feedTitle: "")]);

        Assert.Empty(result.Matches);
        Assert.Empty(result.Ties);
    }

    [Fact]
    public void Should_Return_Nothing_When_The_Schedule_Is_Empty()
    {
        var result = ScheduleMatching.Match([], [Candidate("a", feedTitle: "", "Romelia Senki")]);

        Assert.Empty(result.Matches);
        Assert.Empty(result.Ties);
    }

    #endregion

    #region Test Helpers

    private static ScheduledShow Show(string title) => new(title);

    private static BindingCandidate Candidate(string seriesId, string feedTitle, params string[] titles) =>
        new(seriesId, feedTitle, [.. titles]);

    #endregion
}
