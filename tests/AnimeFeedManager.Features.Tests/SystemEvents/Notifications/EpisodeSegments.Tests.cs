using AnimeFeedManager.Features.SystemEvents.Notifications;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Notifications;

public class EpisodeSegmentsTests
{
    [Fact]
    public void Should_Collapse_Into_One_Range_When_Episodes_Are_Contiguous()
    {
        var coverage = EpisodeSegments.From(["01", "02", "03", "04"]);

        Assert.Equal([new EpisodeRange(1, 4)], coverage.Segments);
        Assert.Equal(4, coverage.Count);
        Assert.Equal(4, coverage.Latest);
    }

    [Fact]
    public void Should_Emit_Gap_Between_Ranges_When_Episodes_Are_Missing()
    {
        var coverage = EpisodeSegments.From(["01", "02", "05", "06", "07"]);

        Assert.Equal(
            [new EpisodeRange(1, 2), new EpisodeGap(3, 4), new EpisodeRange(5, 7)],
            coverage.Segments);
        Assert.Equal(5, coverage.Count);
    }

    [Fact]
    public void Should_Emit_Leading_Gap_When_First_Episode_Is_After_One()
    {
        var coverage = EpisodeSegments.From(["13", "14", "15"]);

        Assert.Equal([new EpisodeGap(1, 12), new EpisodeRange(13, 15)], coverage.Segments);
    }

    [Fact]
    public void Should_Not_Emit_Leading_Gap_When_Series_Starts_At_Zero()
    {
        var coverage = EpisodeSegments.From(["00", "01", "02"]);

        Assert.Equal([new EpisodeRange(0, 2)], coverage.Segments);
        Assert.Equal(3, coverage.Count);
    }

    [Fact]
    public void Should_Keep_Range_Intact_When_Rerelease_Sits_Alongside_Base_Episode()
    {
        var coverage = EpisodeSegments.From(["01", "02", "03", "04", "04v2", "05"]);

        Assert.Equal([new EpisodeRange(1, 5)], coverage.Segments);
        Assert.Equal(["04v2"], coverage.Rereleases);
        Assert.Equal(5, coverage.Count);
    }

    [Fact]
    public void Should_Cover_Episode_Number_When_Only_The_Rerelease_Was_Notified()
    {
        var coverage = EpisodeSegments.From(["01", "02", "03v2", "04"]);

        Assert.Equal([new EpisodeRange(1, 4)], coverage.Segments);
        Assert.Equal(["03v2"], coverage.Rereleases);
    }

    [Fact]
    public void Should_Order_Numerically_When_Episodes_Pass_Two_Digits()
    {
        var coverage = EpisodeSegments.From(["100", "99", "101", "98"]);

        Assert.Equal([new EpisodeGap(1, 97), new EpisodeRange(98, 101)], coverage.Segments);
        Assert.Equal(101, coverage.Latest);
    }

    [Fact]
    public void Should_Count_Each_Episode_Once_When_Input_Has_Duplicates()
    {
        var coverage = EpisodeSegments.From(["01", "01", "02", "02"]);

        Assert.Equal([new EpisodeRange(1, 2)], coverage.Segments);
        Assert.Equal(2, coverage.Count);
    }

    [Fact]
    public void Should_Order_Rereleases_By_Episode_When_Several_Exist()
    {
        var coverage = EpisodeSegments.From(["11v2", "11", "04", "04v3", "04v2"]);

        Assert.Equal(["04v2", "04v3", "11v2"], coverage.Rereleases);
    }

    [Fact]
    public void Should_Skip_Entries_When_They_Do_Not_Match_The_Episode_Format()
    {
        var coverage = EpisodeSegments.From(["01", "", " ", "v2", "special", "02"]);

        Assert.Equal([new EpisodeRange(1, 2)], coverage.Segments);
        Assert.Equal(2, coverage.Count);
    }

    [Fact]
    public void Should_Return_Empty_When_No_Episodes_Are_Given()
    {
        var coverage = EpisodeSegments.From([]);

        Assert.Equal(EpisodeCoverage.Empty, coverage);
    }
}
