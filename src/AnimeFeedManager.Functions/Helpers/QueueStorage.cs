using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace AnimeFeedManager.Functions.Helpers
{
    public class QueueStorage
    {
        public static void StoreInQueue<T>(
            IImmutableList<T> data,
            IAsyncCollector<T> queueCollector,
            ILogger log,
            Func<T, string> logTrace)
        {
            data.AsParallel().ForAll(x =>
            {
                log.LogInformation(logTrace(x));
                queueCollector.AddAsync(x);
            });
        }
    }
}
