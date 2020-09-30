using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class RemoveInterested
    {
        private readonly IMediator _mediator;

        public RemoveInterested(IMediator mediator) => _mediator = mediator;

        [FunctionName("RemoveInterested")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "RemoveInterested")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var command = JsonConvert.DeserializeObject<Application.Subscriptions.Commands.RemoveInterested>(requestBody);

            return await _mediator.Send(command).ToActionResult(log);
        }
    }
}
