using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.Interface;
using LanguageExt;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Services.Collectors.Erai
{
    public class FeedTitles : IAsyncFeedTitlesProvider
    {
        private readonly IBrowserProvider _browser;
        private const string EraiSchedule = "https://spa.erai-raws.info/schedule/";

        public FeedTitles(IBrowserProvider browser)
        {
            _browser = browser;
        }

        public async Task<Either<DomainError, ImmutableList<string>>> GetTitles()
        {
            try
            {
                var browser = await _browser.GetBrowser();
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync(EraiSchedule);
                await page.WaitForSelectorAsync("body.multiple-domain-spa-erai-raws-info");
                const string jsSelection = @"() => {
                        return Array.from(document.querySelectorAll('h6.button.button5.hhhh5 a')).map(x => x.innerText);
                    }";

                var results = await page.EvaluateFunctionAsync<string[]>(jsSelection);
                return Right<DomainError, ImmutableList<string>>(results.ToImmutableList());
            }
            catch (Exception e)
            {
                return Left<DomainError, ImmutableList<string>>(ExceptionError.FromException(e, "Erai_Scraping_Exception"));
            }
        }
    }
}