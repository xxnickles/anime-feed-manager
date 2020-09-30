using System.IO;
using System.Threading.Tasks;
using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AnimeFeedManager.Functions
{
    public class AddInterested
    {
        private readonly IMediator _mediator;

        public AddInterested(IMediator mediator) => _mediator = mediator;
        
        [FunctionName("MergeInterested")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "interested")]
            HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var command = JsonConvert.DeserializeObject<MergeInterestedSeries>(requestBody);

            return await _mediator.Send(command).ToActionResult(log);
        }
    }
}