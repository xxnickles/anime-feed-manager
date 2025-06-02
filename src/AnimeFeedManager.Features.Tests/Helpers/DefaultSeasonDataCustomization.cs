namespace AnimeFeedManager.Features.Tests.Helpers;

public class DefaultSeasonDataCustomization : ICustomization
{
    private static readonly Season[] ValidSeasons = [Season.Winter, Season.Spring, Season.Summer, Season.Fall];
    private static readonly Random Random = new();
    private readonly Year _year = Year.FromNumber(DateTime.Now.Year);
    private readonly Season _season = ValidSeasons[Random.Next(ValidSeasons.Length)];

    public DefaultSeasonDataCustomization(){ }

    public DefaultSeasonDataCustomization(Season season, Year year)
    {
        _season = season;
        _year = year;
    }
    
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Season>(c => c
            .FromFactory(() => {
                // Use the FromString method that AutoFixture is trying to use, 
                // but with a valid season string
                string validSeason = GetRandomSeason();
                return Season.FromString(validSeason);
            }));
        
        
        fixture.Customize<Year>(c => c
            .FromFactory(() => _year));
        
        
        // Customize Season to be one of the valid values
        // fixture.Customize<SeriesSeason>(c => c
        //     .FromFactory(() =>
        //     {
        //         var season = GetRandomSeason();
        //         var year = Year.FromNumber(DateTime.Now.Year);
        //
        //         return new SeriesSeason(season, year);
        //     }));




        //
        // // If AnimeInfoStorage needs customization
        // fixture.Customize<AnimeInfoStorage>(c => c
        //     .With(x => x.Season, "Spring")
        //     .With(x => x.Year, (ushort) DateTime.Now.Year));
    }
    
    private Season GetRandomSeason()
    {
        return ValidSeasons[Random.Next(ValidSeasons.Length)];
    }

}