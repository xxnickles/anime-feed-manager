using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Types;
using AnimeFeedManager.WebApp.Components.AnimeList;

namespace AnimeFeedManager.WebApp.Tests.Components
{
    public class YearSelectorComponentTests : MudBlazorBaseTestContext
    {
        [Fact]
        public void Should_Render_Defaults_When_No_Data()
        {
            var cut = RenderComponent<SeasonSelector>();
            var input =  cut.Find("span.mud-chip-content");
            input.TextContent.Should().Be("No Season Data Available", "No provided data should show default message");
        }


        [Fact]
        public void Should_Render_Defaults_When_Data()
        {
            var seasons = new List<SimpleSeasonInfo>
            {
                new(Season.Fall.ToString(), 2022, false)
            }.ToImmutableList();
        
            var cut = RenderComponent<SeasonSelector>(param =>
                param.Add(p => p.AvailableSeasons, seasons)
            );
            cut.FindAll("h4").Count.Should().Be(1, "Should Render 1 text");
    
            var buttons = cut.FindAll("button");
            buttons.Count.Should().Be(2, "Should Render 2 buttons");

            var backButton = buttons[0];
            var forwardButton = buttons[1];
            backButton.Attributes["disabled"].Should().NotBeNull("Back button should disabled");
            forwardButton.Attributes["disabled"].Should().NotBeNull("Forward Button button should disabled");
        }
    
        [Fact]
        public void Back_Button_Should_Be_Enabled_When_There_is_More_Than_An_Element()
        {
            var seasons = new List<SimpleSeasonInfo>
            {
                new(Season.Fall.ToString(), 2022, false),
                new(Season.Summer.ToString(), 2022, false),
                new(Season.Spring.ToString(), 2022, false)
            }.ToImmutableList();
        
            var cut = RenderComponent<SeasonSelector>(param =>
                param.Add(p => p.AvailableSeasons, seasons)
            );
        
            var buttons = cut.FindAll("button");
            buttons.Count.Should().Be(2, "Should Render 2 buttons");

            var backButton = buttons[0];
            var forwardButton = buttons[1];
            backButton.Attributes["disabled"].Should().BeNull("Back button should enabled");
            forwardButton.Attributes["disabled"].Should().NotBeNull("Forward Button button should disabled");
        }
    
    
        [Fact]
        public void Should_be_In_ExpectedState_After_First_Interaction()
        {
            SimpleSeasonInfo? selectedSeason = null;

            void SelectionHandler(SimpleSeasonInfo dto)
            {
                selectedSeason = dto;
            }

            var seasons = new List<SimpleSeasonInfo>
            {
                new(Season.Fall.ToString(), 2022, false),
                new(Season.Summer.ToString(), 2022, false),
                new(Season.Spring.ToString(), 2022, false)
            }.ToImmutableList();
        
            var cut = RenderComponent<SeasonSelector>(param =>
                param.Add(p => p.AvailableSeasons, seasons)
                    .Add(p => p.SelectedSeasonChanged, (Action<SimpleSeasonInfo>) SelectionHandler)
            
            );
        
            var buttons = cut.FindAll("button");
            buttons.Count.Should().Be(2, "Should Render 2 buttons");

            var backButton = buttons[0];
            var forwardButton = buttons[1];
        
            // Going backwards
            backButton.Click();

            buttons = cut.FindAll("button");
            backButton = buttons[0];
            forwardButton = buttons[1];

            selectedSeason.Should().Be(new SimpleSeasonInfo(Season.Summer.ToString(), 2022, false), "Should match expoected value") ;
            backButton.Attributes["disabled"].Should().BeNull("Back button should enabled");
            forwardButton.Attributes["disabled"].Should().BeNull("Forward Button button should enabled");
        }
    }
}