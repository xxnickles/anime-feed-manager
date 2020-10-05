using AnimeFeedManager.Application.Feed.Commands;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class ProcessTitles
    {
        private readonly IMediator _mediator;

        public ProcessTitles(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName("ProcessTitles")]
        public async Task Run(
            [BlobTrigger("feed-titles-process/{name}", Connection = "AzureWebJobsStorage")] string contents,
            string name,
            ILogger log)
        {
            var command = JsonConvert.DeserializeObject<AddTitles>(contents);

            var result = await _mediator.Send(command);
            result.Match(
                r => log.LogInformation("Titles have been updated"),
                e => log.LogError($"An error occurred while storing feed titles {e.ToString()}"));
        }
    }
}
