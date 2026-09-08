namespace AnimeFeedManager.Features.Scrapping.Types;

public record SeasonParameters(string Season, int Year);

public abstract record SeasonSelector;

/// <summary>Resolve the season to scrape from currently-airing data. Says nothing about which season is featured.</summary>
public record Current : SeasonSelector;

public record BySeason(Season Season, Year Year) : SeasonSelector;
