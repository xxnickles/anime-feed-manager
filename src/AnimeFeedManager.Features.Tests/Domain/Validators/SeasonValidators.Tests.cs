using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Validators;

namespace AnimeFeedManager.Features.Tests.Domain.Validators;

public class SeasonValidatorsTests
{
    [Theory]
    [InlineData("Summer", 0, new []{ "Year" })]
    [InlineData("Winter", 1, new []{ "Year" })]
    [InlineData("Autumn", 1999, new []{ "Year" })]
    [InlineData("Fall", 0, new []{ "Year" })]
    [InlineData("Spring", 0, new []{ "Year" })]
    [InlineData("bad", 2023, new []{ "Season" })]
    [InlineData("bad", 10, new []{ "Year", "Season" })]
    public void Should_Invalidate_Wrong_Inputs(string season, ushort year, string[] expectedErrorFields)
    {
        var sut = SeasonValidators.Parse(season, year);
        sut.Match(
            _ => Assert.Fail("Should not be a positive result"),
            e =>
            {
                Assert.IsType<ValidationErrors>(e);
                var errors = (ValidationErrors)e;
                Assert.Equal(expectedErrorFields, errors.Errors.Keys);
            });
    }

    [Fact]
    public void Season_Validator_Should_Respect_Lower_Boundary()
    {
        // Cannot ve lower than 2000
        var sut = SeasonValidators.Parse("Spring", 1999);
        sut.Match(
            _ => Assert.Fail("Should not be a positive result"),
            e =>
            {
                Assert.IsType<ValidationErrors>(e);
                var errors = (ValidationErrors)e;
                Assert.Equal(["Year"], errors.Errors.Keys);
            });
    }
    
    [Fact]
    public void Season_Validator_Should_Respect_Upper_Boundary()
    {
        // Cannot ve lower than 20009
        var sut = SeasonValidators.Parse("Summer", (ushort)(DateTime.Now.Year + 2));
        // or larger than the next year
        sut.Match(
            _ => Assert.Fail("Should not be a positive result"),
            e =>
            {
                Assert.IsType<ValidationErrors>(e);
                var errors = (ValidationErrors)e;
                Assert.Equal(["Year"], errors.Errors.Keys);
            });
    }
}