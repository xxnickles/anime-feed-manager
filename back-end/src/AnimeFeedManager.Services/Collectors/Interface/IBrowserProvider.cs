using System.Threading.Tasks;
using PuppeteerSharp;

namespace AnimeFeedManager.Services.Collectors.Interface
{
    public interface IBrowserProvider
    {
        Task<Browser> GetBrowser();
    }
}