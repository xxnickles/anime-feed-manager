using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.Library.Storage
{
    public class AlternativeTitlesDataTests
    {
        #region Parse Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_Empty_When_Stored_Value_Has_No_Content(string? stored)
        {
            var result = StoredAlternativeTitles.Parse(stored);

            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void Should_Read_Provider_Titles_When_Stored_Value_Is_Json()
        {
            var stored = """
                         {"romaji":"Ranma ½","english":"Ranma 1/2","native":"らんま1/2",
                          "synonyms":["Ranma One Half"],"user":["Ranma"]}
                         """;

            var result = StoredAlternativeTitles.Parse(stored);

            Assert.Equal("Ranma ½", result.Romaji);
            Assert.Equal("Ranma 1/2", result.English);
            Assert.Equal("らんま1/2", result.Native);
            Assert.Equal(["Ranma One Half"], result.Synonyms!);
        }

        [Theory]
        [InlineData("Alt Title 1|Alt Title 2")]
        [InlineData("Sole Title")]
        [InlineData("{ this never closes")]
        public void Should_Return_Empty_When_Stored_Value_Is_Not_A_Json_Object(string stored)
        {
            var result = StoredAlternativeTitles.Parse(stored);

            Assert.True(result.IsEmpty);
        }

        #endregion

        #region ForMatching Tests

        [Fact]
        public void Should_Order_English_Then_Synonyms()
        {
            var titles = new AlternativeTitlesData(
                English: "English Title",
                Synonyms: ["Synonym A", "Synonym B"]);

            Assert.Equal(["English Title", "Synonym A", "Synonym B"], titles.ForMatching);
        }

        [Fact]
        public void Should_Exclude_Romaji_And_Native()
        {
            var titles = new AlternativeTitlesData(
                Romaji: "Romaji Title",
                Native: "ネイティブ",
                English: "English Title");

            Assert.Equal(["English Title"], titles.ForMatching);
        }

        [Fact]
        public void Should_Exclude_Abbreviation()
        {
            var titles = new AlternativeTitlesData(English: "English Title", Abbreviation: "ET");

            Assert.Equal(["English Title"], titles.ForMatching);
        }

        [Fact]
        public void Should_Drop_Duplicates_Ignoring_Case_Keeping_English_First()
        {
            var titles = new AlternativeTitlesData(
                English: "SPY x FAMILY",
                Synonyms: ["spy x family", "Spy Family"]);

            Assert.Equal(["SPY x FAMILY", "Spy Family"], titles.ForMatching);
        }

        [Fact]
        public void Should_Drop_Blank_Entries()
        {
            var titles = new AlternativeTitlesData(
                English: "  ",
                Synonyms: ["", "   ", "Real Synonym"]);

            Assert.Equal(["Real Synonym"], titles.ForMatching);
        }

        [Fact]
        public void Should_Return_Empty_When_Only_Unmatchable_Slots_Are_Filled()
        {
            var titles = new AlternativeTitlesData(Romaji: "Romaji Title", Native: "ネイティブ");

            Assert.Empty(titles.ForMatching);
        }

        #endregion

        #region ToStoredString Tests

        [Fact]
        public void Should_Store_Nothing_When_Every_Slot_Is_Empty()
        {
            Assert.Equal(string.Empty, AlternativeTitlesData.Empty.ToStoredString());
            Assert.Equal(string.Empty, new AlternativeTitlesData(Synonyms: []).ToStoredString());
        }

        [Fact]
        public void Should_Store_Abbreviation_When_It_Is_The_Only_Slot()
        {
            var titles = new AlternativeTitlesData(Abbreviation: "ET");

            Assert.False(titles.IsEmpty);
            Assert.Equal("ET", StoredAlternativeTitles.Parse(titles.ToStoredString()).Abbreviation);
        }

        [Fact]
        public void Should_Omit_Absent_Slots_From_Stored_Value()
        {
            var stored = new AlternativeTitlesData(English: "Only English").ToStoredString();

            Assert.DoesNotContain("romaji", stored);
            Assert.DoesNotContain("null", stored);
            Assert.Contains("Only English", stored);
        }

        [Fact]
        public void Should_Survive_A_Round_Trip_Through_Parse()
        {
            var original = new AlternativeTitlesData(
                Romaji: "Romaji Title",
                English: "English Title",
                Native: "ネイティブ",
                Synonyms: ["Synonym A"],
                Abbreviation: "RT");

            var result = StoredAlternativeTitles.Parse(original.ToStoredString());

            Assert.Equal(original.Romaji, result.Romaji);
            Assert.Equal(original.English, result.English);
            Assert.Equal(original.Native, result.Native);
            Assert.Equal(original.Synonyms, result.Synonyms);
            Assert.Equal(original.Abbreviation, result.Abbreviation);
        }

        #endregion
    }
}
