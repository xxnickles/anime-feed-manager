using AnimeFeedManager.Features.Library.Airing;
using AnimeFeedManager.Features.Library.Events;
using AnimeFeedManager.Features.Library.Images;
using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Features.Library.Import.Jikan.Registration;
using AnimeFeedManager.Infrastructure.Eventing;
using AnimeFeedManager.Infrastructure.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Library.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers the Library feature: the Jikan HTTP client, the
        /// <see cref="LibraryImport"/> service, and the
        /// <see cref="LibraryImportCronJob"/> that triggers a weekly import via the job executor.
        /// Cover images are stored inline during import via the <see cref="IImageHttpClient"/>;
        /// the <see cref="ImagesContainerInitializer"/> provisions the blob container at startup.
        ///
        /// Depends on the host having already registered the cron-scheduler/job-executor
        /// (<c>AddCronScheduler()</c>), the Cosmos container factory
        /// (<c>AddCosmosInfrastructure(...)</c>), and the <c>BlobServiceClient</c>
        /// (<c>AddAzureBlobServiceClient("blobs")</c>).
        /// </summary>
        public IHostApplicationBuilder AddLibrary()
        {
            builder.AddJikanClient();

            builder.Services.AddScoped<LibraryImportJob>();
            builder.Services
                .AddCronJob<LibraryImportCronJob>();
            builder.Services.AddEventHandler<OperationFailed, LibraryEventHandler>();
            builder.Services.AddEventHandler<SeasonImported, SeasonImportedEventHandler>();

            builder.Services.AddScoped<AiringSeriesIndexRefreshJob>();
            builder.Services.AddCronJob<AiringSeriesIndexRefreshCronJob>();
            builder.Services.AddEventHandler<SeasonImported, AiringSeriesIndexRefreshEventHandler>();

            builder.Services.AddHttpClient<IImageHttpClient, ImageHttpClient>();
            builder.Services.AddHostedService<ImagesContainerInitializer>();

            return builder;
        }
    }
}
