﻿namespace AnimeFeedManager.Web.Features.Common.Filters;

public sealed class DelayFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        await Task.Delay(2000);
        return await next(context);
    }
}