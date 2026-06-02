namespace AnimeFeedManager.Features.Library.Images;

/// <summary>
/// Downloads the cover named by a <see cref="ProcessSeriesImageCommand"/> and stores it in blob
/// storage, yielding the relative blob path to persist as the series' <c>CoverImageUrl</c>. Built
/// from <see cref="ImageProcessorDependencies"/> via <see cref="ImageProcessor.SeriesImageProcessorHandler"/>.
/// </summary>
public delegate Task<Result<string>> SeriesImageProcessor(
    ProcessSeriesImageCommand command,
    CancellationToken cancellationToken);
