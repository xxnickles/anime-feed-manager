using AnimeFeedManager.Features.Auth.Registration;

namespace AnimeFeedManager.Features.Tests.Auth.Registration;

public class RegistrationExtensionsTests
{
    [Fact]
    public void Should_Return_NewRegistration_When_Email_And_DisplayName_Are_Valid()
    {
        var result = ("nick@example.com", "Nick").AsUserRegistration();

        result.Match(
            onOk: reg =>
            {
                Assert.Equal("nick@example.com", (string)reg.Email);
                Assert.Equal("Nick", (string)reg.DisplayName);
                Assert.False(string.IsNullOrWhiteSpace(reg.UserId));
            },
            onError: error => Assert.Fail($"Expected success but got: {error.Message}"));
    }

    [Fact]
    public void Should_Mint_A_Unique_UserId_Per_Registration()
    {
        var first = ("nick@example.com", "Nick").AsUserRegistration().MatchToValue(reg => reg.UserId, _ => string.Empty);
        var second = ("nick@example.com", "Nick").AsUserRegistration().MatchToValue(reg => reg.UserId, _ => string.Empty);

        Assert.NotEmpty(first);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Invalid()
    {
        var result = ("not-an-email", "Nick").AsUserRegistration();

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_When_DisplayName_Is_Blank(string displayName)
    {
        var result = ("nick@example.com", displayName).AsUserRegistration();

        Assert.True(result.IsFailure);
    }
}
