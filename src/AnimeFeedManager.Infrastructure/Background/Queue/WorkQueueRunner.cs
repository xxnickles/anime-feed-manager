using AnimeFeedManager.Shared.Results.Static;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace AnimeFeedManager.Infrastructure.Background.Queue;

/// <summary>
/// Drain-loop helpers used by the closures built in <c>AddWorkQueueHandler</c>.
/// Lives outside the registration extension so the generic type parameters
/// <c>TCommand</c> / <c>THandler</c> stay first-class — no <see cref="object"/>
/// erasure of command data, no reflection at fire time.
/// </summary>
internal static class WorkQueueRunner
{
    internal static async Task DrainLoop<TCommand, THandler>(
        ChannelReader<TCommand> reader,
        IServiceScopeFactory scopes,
        ResiliencePipeline defaultPipeline,
        CancellationToken stoppingToken)
        where THandler : WorkHandler<TCommand>
    {
        try
        {
            await foreach (var command in reader.ReadAllAsync(stoppingToken))
            {
                await ProcessOne<TCommand, THandler>(command, scopes, defaultPipeline, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown — expected.
        }
    }

    private static async Task ProcessOne<TCommand, THandler>(
        TCommand command,
        IServiceScopeFactory scopes,
        ResiliencePipeline defaultPipeline,
        CancellationToken stoppingToken)
        where THandler : WorkHandler<TCommand>
    {
        await using var scope = scopes.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        var pipeline = handler.ResiliencePipeline ?? defaultPipeline;
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<THandler>>();

        try
        {
            await pipeline.ExecuteAsync(async token =>
            {
                var result = await handler.Handle(command, token);
                result.Match(
                    onOk: _ => { },
                    onError: error => throw new WorkHandlerFailureException(error));
            }, stoppingToken);
        }
        catch (WorkHandlerFailureException ex)
        {
            logger.LogError(
                "Work handler {Handler} failed after retries: {Error}",
                typeof(THandler).Name, ex.Error.Message);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown — expected.
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Work handler {Handler} threw an unhandled exception.",
                typeof(THandler).Name);
        }
    }
}
