namespace AnimeFeedManager.Features.AniDb
{
    internal record JsonSeasonInfo(string Season, int Year);

    internal record JsonAnimeInfo(string Title, string? ImageUrl, string Synopsys, string Date,
        JsonSeasonInfo SeasonInfo);

    internal record SeriesContainer(string Id, string Title, string? ImageUrl, string Synopsys, string Date,
        JsonSeasonInfo SeasonInfo);

    internal record ScrapResult(IEnumerable<SeriesContainer> Series, JsonSeasonInfo Season);


    internal static class Constants {
        internal const string ScrappingScript = @"
        () => {
            const seasonInfomationGetter = () => {
                const getDate = (str) => str.includes('/') ? str.split('/')[1] : str;
                const formatSeason = (str) => str === 'autumn' ? 'fall' : str;
                const titleParts = document.querySelector('div.g_section.content > h2 span').innerText.split(' ');
                return {
                    season: formatSeason(titleParts[0].toLowerCase()),
                    year: parseInt(getDate(titleParts[1]))
                }
            }

            const seasonInfomation = seasonInfomationGetter();

            return [].slice.call(document.querySelectorAll('div.g_bubble.box'))
                .map(card => {

                    const getImage = () => {
                        const cleanSrc = (src) => src.replace('.jpg-thumb', '')
                        const image = card.querySelector('div.thumb.image img');
                        return cleanSrc(image.src);
                    }

                    const data = card.querySelector('div.data');
                    const title = data.querySelector('div.wrap.name a').innerText;
                    const synopsys = data.querySelector('div.desc')?.innerText ?? '';
                    const date = data.querySelector('div.date').innerText;

                    return {
                        title,
                        imageUrl: getImage(),
                        synopsys,
                        date,
                        seasonInfo: seasonInfomation
                    }
                }
                );
         }
    ";
    }
}