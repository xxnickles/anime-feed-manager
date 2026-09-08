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
        public void Should_Read_Both_Blocks_When_Stored_Value_Is_Json()
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
            Assert.Equal(["Ranma"], result.User!);
        }

        [Fact]
        public void Should_Read_Titles_Into_User_When_Stored_Value_Is_Legacy_List()
        {
            var result = StoredAlternativeTitles.Parse("Alt Title 1|Alt Title 2");

            Assert.Equal(["Alt Title 1", "Alt Title 2"], result.User!);
            Assert.Null(result.English);
            Assert.Null(result.Synonyms);
        }

        [Fact]
        public void Should_Read_Title_Into_User_When_Legacy_Value_Has_No_Separator()
        {
            var result = StoredAlternativeTitles.Parse("Sole Title");

            Assert.Equal(["Sole Title"], result.User!);
        }

        [Fact]
        public void Should_Fall_Back_To_Legacy_When_Json_Is_Malformed()
        {
            var result = StoredAlternativeTitles.Parse("{ this never closes");

            Assert.Equal(["{ this never closes"], result.User!);
        }

        #endregion

        #region ForMatching Tests

        [Fact]
        public void Should_Order_User_Then_English_Then_Synonyms()
        {
            var titles = new AlternativeTitlesData(
                English: "English Title",
                Synonyms: ["Synonym A", "Synonym B"],
                User: ["User Title"]);

            Assert.Equal(
                ["User Title", "English Title", "Synonym A", "Synonym B"],
                titles.ForMatching);
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
        public void Should_Drop_Duplicates_Ignoring_Case_Keeping_User_First()
        {
            var titles = new AlternativeTitlesData(
                English: "SPY x FAMILY",
                Synonyms: ["spy x family", "Spy Family"],
                User: ["Spy X Family"]);

            Assert.Equal(["Spy X Family", "Spy Family"], titles.ForMatching);
        }

        [Fact]
        public void Should_Drop_Blank_Entries()
        {
            var titles = new AlternativeTitlesData(
                English: "  ",
                Synonyms: ["", "Real Synonym"],
                User: ["   "]);

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
            Assert.Equal(string.Empty, new AlternativeTitlesData(Synonyms: [], User: []).ToStoredString());
        }

        [Fact]
        public void Should_Omit_Absent_Slots_From_Stored_Value()
        {
            var stored = new AlternativeTitlesData(User: ["Only User"]).ToStoredString();

            Assert.DoesNotContain("romaji", stored);
            Assert.DoesNotContain("null", stored);
            Assert.Contains("Only User", stored);
        }

        [Fact]
        public void Should_Survive_A_Round_Trip_Through_Parse()
        {
            var original = new AlternativeTitlesData(
                Romaji: "Romaji Title",
                English: "English Title",
                Native: "ネイティブ",
                Synonyms: ["Synonym A"],
                User: ["User Title"]);

            var result = StoredAlternativeTitles.Parse(original.ToStoredString());

            Assert.Equal(original.Romaji, result.Romaji);
            Assert.Equal(original.English, result.English);
            Assert.Equal(original.Native, result.Native);
            Assert.Equal(original.Synonyms, result.Synonyms);
            Assert.Equal(original.User, result.User);
        }

        [Fact]
        public void Should_Keep_The_Provider_Block_When_Only_User_Titles_Are_Rewritten()
        {
            var scrapped = new AlternativeTitlesData(
                English: "English Title",
                Synonyms: ["Synonym A"]).ToStoredString();

            var edited = (StoredAlternativeTitles.Parse(scrapped) with { User = ["Curated"] }).ToStoredString();
            var result = StoredAlternativeTitles.Parse(edited);

            Assert.Equal("English Title", result.English);
            Assert.Equal(["Synonym A"], result.Synonyms!);
            Assert.Equal(["Curated"], result.User!);
        }

        #endregion
    }
}
