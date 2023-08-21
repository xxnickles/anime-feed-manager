using AnimeFeedManager.Features.Domain.Errors;
using AnimeFeedManager.Features.Domain.Types;
using AnimeFeedManager.Features.Domain.Validators;

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
    [InlineData("bad", 10, new []{ "Season", "Year" })]
    public void Should_Invalidate_Wrong_Inputs(string season, ushort year, string[] expectedErrorFields)
    {
        var sut = SeasonValidators.Validate(season, year);
        sut.Match(
            _ => Assert.Fail("Should not be a positive result"),
            e =>
            {
                e.Should().BeOfType<ValidationErrors>();
                var errors = (ValidationErrors)e;
                errors.Errors.Keys.Should().BeEquivalentTo(expectedErrorFields);
            });
    }

    [Fact]
    public void Season_Validator_Should_Respect_Lower_Boundary()
    {
        // Cannot ve lower than 2000
        var sut = SeasonValidators.Validate("Spring", 1999);
        sut.Match(
            _ => Assert.Fail("Should not be a positive result"),
            e =>
            {
                e.Should().BeOfType<ValidationErrors>();
                var errors = (ValidationErrors)e;
                errors.Errors.Keys.Should().ContainSingle(error => error == "Year");
            });
    }
    
    [Fact]
    public void Season_Validator_Should_Respect_Uper_Boundary()
    {
        // Cannot ve lower than 20009
        var sut = SeasonValidators.Validate("Summer", (ushort)(DateTime.Now.Year + 2));
        // or larger than the next year
        sut.Match(
            _ => Assert.Fail("Should not be a positive result"),
            e =>
            {
                e.Should().BeOfType<ValidationErrors>();
                var errors = (ValidationErrors)e;
                errors.Errors.Keys.Should().ContainSingle(error => error == "Year");
            });
    }

    [Fact]
    public void Should_Always_Pass_Latest()
    {
        var sut = SeasonValidators.Validate(new Latest());
        sut.Match(
            selector => selector.Should().BeOfType<Latest>(),
            _ => Assert.Fail("Should not be an error")
        );
    }
    
    [Fact]
    public void Should_Always_Pass_A_Valid_Season_Selector()
    {
        var sut = SeasonValidators.Validate(new BySeason("Summer", 2010));
        sut.Match(
            selector => selector.Should().BeOfType<BySeason>(),
            _ => Assert.Fail("Should not be an error")
        );
    }
}