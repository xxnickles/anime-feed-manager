using AnimeFeedManager.Core.ConstrainedTypes;
using Xunit;

namespace AnimeFeedManager.Application.Test.ConstrainedTypes
{
    public class SeasonTest
    {
        [Theory]
        [MemberData(nameof(GreaterTestData))]
        public void GreaterOperatorShouldReturnTrue(Season a, Season b)
        {
            Assert.True(a > b);
        }

        [Theory]
        [MemberData(nameof(GreaterWrongTestData))]
        public void GreaterOperatorShouldReturnFalse(Season a, Season b)
        {
            Assert.False(a > b);
        }

        [Theory]
        [MemberData(nameof(LowerTestData))]
        public void LowerOperatorShouldReturnTrue(Season a, Season b)
        {
            Assert.True(a < b);
        }

        [Theory]
        [MemberData(nameof(LowerWrongTestData))]
        public void LowerOperatorShouldReturnFalse(Season a, Season b)
        {
            Assert.False(a < b);
        }

        public static TheoryData<Season, Season> GreaterTestData => 
        new TheoryData<Season, Season>
        {
            { Season.Spring, Season.Winter },
            { Season.Summer, Season.Winter },
            { Season.Fall, Season.Winter },

            { Season.Summer, Season.Spring },
            { Season.Fall, Season.Spring },
        
            { Season.Fall, Season.Summer }
        };

        public static TheoryData<Season, Season> GreaterWrongTestData =>
            new TheoryData<Season, Season>
            {
                { Season.Spring, Season.Spring },
                { Season.Summer, Season.Fall },
                { Season.Winter, Season.Fall },
                { Season.Fall, Season.Fall },
            };
       

        public static TheoryData<Season, Season> LowerTestData =>
            new TheoryData<Season, Season>
            {
                {  Season.Winter, Season.Spring },
                {  Season.Winter, Season.Fall },
                {  Season.Winter, Season.Summer },

                { Season.Spring, Season.Summer },
                { Season.Spring, Season.Fall },

                { Season.Summer, Season.Fall }
            };

        public static TheoryData<Season, Season> LowerWrongTestData =>
            new TheoryData<Season, Season>
            {
                { Season.Spring, Season.Spring },
                { Season.Fall, Season.Summer },
                { Season.Fall, Season.Winter },
                { Season.Fall, Season.Fall },
            };
    }
}