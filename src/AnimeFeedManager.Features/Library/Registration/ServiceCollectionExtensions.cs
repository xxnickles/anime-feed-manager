using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Features.Library.Import.Jikan.Registration;
using AnimeFeedManager.Infrastructure.Registration;

namespace AnimeFeedManager.Features.Library.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers the Library feature: the Jikan HTTP client, the
        /// <see cref="LibraryImportHandler"/> and its
        /// <see cref="WorkQueue{TCommand}"/> for
        /// <see cref="LibraryImportCommand"/>, and the
        /// <see cref="LibraryImportCronJob"/> that enqueues a weekly import.
        ///
        /// Depends on the host having already registered the cron-scheduler and
        /// work-queue processor infrastructure (<c>AddCronScheduler()</c>,
        /// <c>AddWorkQueueProcessor()</c>) and the Cosmos container factory
        /// (<c>AddCosmosInfrastructure(...)</c>).
        /// </summary>
        public IHostApplicationBuilder AddLibrary()
        {
            builder.AddJikanClient();

            builder.Services
                .AddWorkQueueHandler<LibraryImportCommand, LibraryImportHandler>()
                .AddCronJob<LibraryImportCronJob>();

            return builder;
        }
    }
}
