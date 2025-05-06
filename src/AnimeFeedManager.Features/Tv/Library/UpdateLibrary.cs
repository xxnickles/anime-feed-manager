using AnimeFeedManager.Features.Scrapping.AniDb;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Messages;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tv.Library;

public sealed class UpdateLibrary
{
    private record ProcessData(
        IEnumerable<AnimeInfoStorage> Series,
        ImmutableList<string> FeedTitles,
        SeriesSeason JsonSeason);

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
                .Bind(tiles => GetProcessData(tiles, season, _puppeteerOptions));


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

    private static async Task<Result<ProcessData>> GetProcessData(
        ImmutableList<string> feedTitles,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions)
    {
        // Scrapping
        var (series, jsonSeason) = await AniDbScrapper.Scrap(CreateScrappingLink(season), puppeteerOptions);

        return (jsonSeason.Season, jsonSeason.Year, season is Latest)
            .ParseAsSeriesSeason()
            .Map(seriesSeason => new ProcessData(series.Select(Transform), feedTitles, seriesSeason));

        // To keep it simple, we are using the season information coming from the scrapping instead of the parsed one
        // But at this point we are sure Season is valid as we have parsed the data scrapped 
        AnimeInfoStorage Transform(SeriesContainer seriesContainer)
        {
            return new AnimeInfoStorage
            {
                RowKey = seriesContainer.Id,
                PartitionKey = IdHelpers.GenerateAnimePartitionKey(jsonSeason.Season, (ushort) jsonSeason.Year),
                Title = seriesContainer.Title,
                Synopsis = seriesContainer.Synopsys,
                FeedTitle = string.Empty,
                Date = Utils.ParseDate(seriesContainer.Date, seriesContainer.SeasonInfo.Year)?.ToUniversalTime(),
                Status = SeriesStatus.NotAvailableValue,
                Season = seriesContainer.SeasonInfo.Season,
                Year = seriesContainer.SeasonInfo.Year
            };
        }
    }


    // private static Result<ProcessData> AddFeedTitle(ProcessData processData)
    // {
    //     
    // } 

    private Task<Result<Unit>> SendUpdateEvent(AnimeInfoStorage series, CancellationToken token = default)
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