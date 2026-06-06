using AnimeFeedManager.Features.Library.Images;
using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Features.Library.Import.Jikan.Registration;
using AnimeFeedManager.Infrastructure.Registration;
using Microsoft.Extensions.DependencyInjection;

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
        /// Also registers the cover-image pipeline: the <see cref="IImageHttpClient"/>,
        /// the <see cref="ProcessSeriesImageHandler"/> work queue, and the
        /// <see cref="ImagesContainerInitializer"/> that provisions the blob container at startup.
        ///
        /// Depends on the host having already registered the cron-scheduler and
        /// work-queue processor infrastructure (<c>AddCronScheduler()</c>,
        /// <c>AddWorkQueueProcessor()</c>), the Cosmos container factory
        /// (<c>AddCosmosInfrastructure(...)</c>), and the <c>BlobServiceClient</c>
        /// (<c>AddAzureBlobServiceClient("blobs")</c>).
        /// </summary>
        public IHostApplicationBuilder AddLibrary()
        {
            builder.AddJikanClient();

            builder.Services
                .AddWorkQueueHandler<LibraryImportCommand, LibraryImportHandler>()
                .AddCronJob<LibraryImportCronJob>();

            builder.Services.AddHttpClient<IImageHttpClient, ImageHttpClient>();
            builder.Services.AddWorkQueueHandler<ProcessSeriesImageCommand, ProcessSeriesImageHandler>();
            builder.Services.AddHostedService<ImagesContainerInitializer>();

            return builder;
        }
    }
}
