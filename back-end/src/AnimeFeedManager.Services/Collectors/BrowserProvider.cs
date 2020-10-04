using System.IO;
using System.Threading.Tasks;
using AnimeFeedManager.Services.Collectors.Interface;
using PuppeteerSharp;

namespace AnimeFeedManager.Services.Collectors
{
    public class BrowserProvider : IBrowserProvider
    {
        public async Task<Browser> GetBrowser()
        {
            var downloadsFolder = Path.GetTempPath(); // Need to use a folder with write permissions. "/tmp/" in Azure 


            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = downloadsFolder
            });

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            return browser;
        }
    }
}