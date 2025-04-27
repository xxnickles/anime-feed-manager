using AnimeFeedManager.Features.Scrapping.AniDb;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Messages;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tv.Library;

public sealed class UpdateLibrary
{
  
    private readonly record struct ProcessData(ImmutableList<AnimeInfoStorage> Series, ImmutableList<string> feedTitles, string JsonSeason);
    
    private readonly ISeasonFeedTitlesProvider _seasonFeedTitlesProvider;
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly ILogger<UpdateLibrary> _logger;

    public UpdateLibrary(
        ISeasonFeedTitlesProvider seasonFeedTitlesProvider,
        IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions, 
        ILogger<UpdateLibrary> logger)
    {
        _seasonFeedTitlesProvider = seasonFeedTitlesProvider;
        _domainPostman = domainPostman;
        _puppeteerOptions = puppeteerOptions;
        _logger = logger;
    }
    
    public async Task<Result<Unit>> Execute(SeasonSelector season, CancellationToken token = default)
    {
        try
        {
            // Get Season Feed Titles
            var t = _seasonFeedTitlesProvider.Get()
                    .Bind(tiles => );
            
            
            // Scrapping
            var (series,jsonSeason) = await AniDbScrapper.Scrap(CreateScrappingLink(season), _puppeteerOptions);
            
            // Send Series Messages
            
            // Create State Process based on messages sent correctly
            
            // Sent 
            
            return Result<Unit>.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when updating TV library");
            return Result<Unit>.Failure(new HandledError());
        }
    }

    private async ValueTask<Result<ProcessData>> GetProcessData(
        ImmutableList<string> feedTitles,
        SeasonSelector season,
        CancellationToken token = default)
    {
        // Scrapping
        var (series,jsonSeason) = await AniDbScrapper.Scrap(CreateScrappingLink(season), _puppeteerOptions);
        var seriesToProcess = series.Select(s => new AnimeInfoStorage
        {
            Date = s.Date,
            
        })
        
        return Result<ProcessData>.Success(new ProcessData());
    }

    private ValueTask<Result<Unit>> SendUpdateEvent(AnimeInfoStorage series, CancellationToken token = default)
    {
        return _domainPostman.SendMessage(new TvSeriesToUpdate(series), token);
    }
    
    private static string CreateScrappingLink(SeasonSelector season)
    {
        return season switch
        {
            Latest => "https://anidb.net/anime/season/?type.tvseries=1",
            BySeason s =>
                $"https://anidb.net/anime/season/{s.Year}/{s.Season.ToAlternativeString()}/?do=calendar&h=1&type.tvseries=1",
            _ => throw new UnreachableException()
        };
    }
}